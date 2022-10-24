using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Controller : NetworkBehaviour
{
    public State state;
    public enum State
    {
        NothingSelected,
        CreatureSelected,
        CreatureInHandSelected,
        SpellInHandSelected,
        PlacingCastle,
        Waiting
    }


    //Could use a state machine if creature is selected change state to creature selected 
    //if card in hand is selected change state to placing card
    //if neither are selected change state to selecting
    //if environment is selected change state to environment selected
    //if environment card is selected change state to environment card selected

    public Dictionary<Vector3Int, BaseTile> tilesOwned = new Dictionary<Vector3Int, BaseTile>();

    MousePositionScript mousePositionScript;

    public Color col;
    public Color transparentCol;


    Vector3 mousePosition;
    TileBase highlightTile;
    Tilemap highlightMap;// set these = to gamemanage.singleton.highlightmap TODO
    Tilemap baseMap;
    Tilemap environmentMap;
    Tilemap waterMap;
    protected Grid grid;
    Vector3Int previousCellPosition;

    Transform castle;
    Transform instantiatedCaste;
    Creature creatureSelected;
    Vector3Int currentLocalHoverCellPosition;
    Vector3Int cellPositionSentToClients;
    Vector3Int targetedCellPosition;

    [SerializeField] LayerMask creatureMask;

    Vector3Int placedCellPosition;

    public int turnTimer;
    int turnThreshold = 80; //todo make this 800
    int maxHandSize = 7;
    [SerializeField] List<CardInHand> cardsInDeck;
    List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardInHand cardSelected;
    public List<Vector3> allVertextPointsInTilesOwned = new List<Vector3>();

    Transform instantiatedPlayerUI;
    Transform cardParent;

    Canvas canvasMain;

    public int tick = 0; //this is for determining basically everything
    public float tickTimer = 0f;
    float tickThreshold = .12f;
    public List<Vector3Int> clickQueueForTick = new List<Vector3Int>();
    List<Vector3Int> tempLocalPositionsToSend = new List<Vector3Int>();
    List<Vector3Int> tempLocalTilePositionPurchased = new List<Vector3Int>();
    List<Vector3Int> localTilePositionPurchasedToSend = new List<Vector3Int>();
    List<int> tempLocalIndecesOfCardsInHand = new List<int>();
    public List<int> IndecesOfCardsInHandQueue = new List<int>();

    public bool hasTickedSinceSendingLastMessage = true;
    Creature locallySelectedCreature;

    PlayerResources resources;

    delegate void ResourcesChanged(PlayerResources resources);
    ResourcesChanged resourcesChanged;

    [SerializeField] Transform playerHud;
    HudElements hudElements;
    delegate void Turn();
    event Turn turn;

    int numOfPurchasableHarvestTiles = 1;

    public List<BaseTile> harvestedTiles = new List<BaseTile>();

    public override void OnNetworkSpawn()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        GrabAllObjectsFromGameManager();
        col = Color.red;
        col = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        transparentCol = col;
        transparentCol.a = .5f;
        SpawnHUDAndHideOnAllNonOwners();
        instantiatedPlayerUI.gameObject.SetActive(false);
        cardsInDeck = GameManager.singleton.Shuffle(cardsInDeck);
        turn += OnTurn;
        resources = new PlayerResources();
        resourcesChanged += UpdateHudForResourcesChanged;
        mousePositionScript = GetComponent<MousePositionScript>();
        state = State.PlacingCastle;

    }

    internal void StartGame()
    {
        if (IsOwner)
        {
            instantiatedPlayerUI.gameObject.SetActive(true);
        }
        for (int i = 0; i < 3; i++)
        {
            DrawCard();
        }
        SetStateToNothingSelected();
    }

    void GrabAllObjectsFromGameManager()
    {
        GameManager.singleton.tick += OnTick;
        canvasMain = FindObjectOfType<Canvas>();
        highlightTile = GameManager.singleton.highlightTile;
        highlightMap = GameManager.singleton.highlightMap;// set these = to gamemanage.singleton.highlightmap TODO
        baseMap = GameManager.singleton.baseMap;
        environmentMap = GameManager.singleton.enviornmentMap;
        waterMap = GameManager.singleton.waterTileMap;
        grid = GameManager.singleton.grid;
        castle = GameManager.singleton.castleTransform;
        GameManager.singleton.playerList.Add(this);
    }
    void SpawnHUDAndHideOnAllNonOwners()
    {
        instantiatedPlayerUI = Instantiate(playerHud, canvasMain.transform);
        cardParent = instantiatedPlayerUI.GetComponent<HudElements>().cardParent;
        if (!IsOwner)
        {
            instantiatedPlayerUI.gameObject.SetActive(false);
        }
        if (IsOwner)
        {
            cardParent.gameObject.GetComponent<Image>().color = transparentCol;
            hudElements = instantiatedPlayerUI.GetComponent<HudElements>();
            hudElements.UpdateHudVisuals(this, turnThreshold);
        }
    }

    private void OnTurn()
    {
        StartTurnPhase();
    }

    public void StartTurnPhase()
    {
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                HandleDrawCards();
                HandleMana();
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureInHandSelected:
                HandleDrawCards();
                HandleMana();
                TriggerAllCreatureAbilities();
                break;
            case State.SpellInHandSelected:
                HandleDrawCards();
                HandleMana();
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureSelected:
                HandleDrawCards();
                HandleMana();
                TriggerAllCreatureAbilities();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        currentLocalHoverCellPosition = grid.WorldToCell(mousePosition);
        mousePosition = mousePositionScript.GetMousePositionWorldPoint();
        if (currentLocalHoverCellPosition != previousCellPosition)
        {
            highlightMap.SetTile(previousCellPosition, null);
            highlightMap.SetTile(currentLocalHoverCellPosition, highlightTile);
            previousCellPosition = currentLocalHoverCellPosition;
            if (locallySelectedCreature != null)
            {
                VisualPathfinderOnCreatureSelected(locallySelectedCreature);
            }
        }

        if (state != State.PlacingCastle)
        {
            HandleSpacebarPressed();
        }
        tickTimer += Time.deltaTime;
        if (hasTickedSinceSendingLastMessage)
        {
            hasTickedSinceSendingLastMessage = false;
            tickTimer = 0f;
            SendAllInputsInQueue();
        }
        if (!hasTickedSinceSendingLastMessage && !GameManager.singleton.playerList.Contains(this)) //attempts to resend on a failed rpc
        {
            Debug.LogError("Failed sending message attempting to try again at tick " + tick);
            hasTickedSinceSendingLastMessage = false;
            tickTimer = 0f;
            SendAllInputsInQueue();
        }
        if (Input.GetMouseButtonDown(0))
        {
            cellPositionSentToClients = grid.WorldToCell(mousePosition);

            if (state == State.NothingSelected)
            {
                if (locallySelectedCreature != null)
                {
                    AddToTickQueueLocal(cellPositionSentToClients);
                    locallySelectedCreature = null;
                    return;
                }
                if (!CheckForRaycast())
                {
                    AddToTickQueueLocal(cellPositionSentToClients);
                }
            }
            else
            {
                if (!CheckForRaycast())
                {
                    AddToTickQueueLocal(cellPositionSentToClients);
                }
            }
            if (state == State.NothingSelected)
            {
                if (ShowingPurchasableHarvestTiles)
                {
                    if (CheckToSeeIfClickedHarvestTileCanBePurchased(cellPositionSentToClients))
                    {
                        AddToPuchaseTileQueueLocal(cellPositionSentToClients);
                    }
                }
            }
            return;
        }

    }


    private void FixedUpdate()
    {
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                HandleTurn();
                break;
            case State.CreatureInHandSelected:
                HandleTurn();
                break;
            case State.SpellInHandSelected:
                HandleTurn();
                break;
            case State.CreatureSelected:
                HandleTurn();
                break;
        }
    }

    bool ShowingPurchasableHarvestTiles = false;
    private void HandleSpacebarPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CameraControl.Singleton.ReturnHome(new Vector3(instantiatedCaste.transform.position.x, instantiatedCaste.transform.position.y, instantiatedCaste.transform.position.z - 7));
            ShowHarvestedTiles();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            CameraControl.Singleton.CancelReturnHome();
            HideHarvestedTiles();
        }
    }

    private void ShowHarvestedTiles()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            bt.Value.ShowHarvestIcon();
        }
        ShowingPurchasableHarvestTiles = true;
    }

    private void HideHarvestedTiles()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            bt.Value.HideHarvestIcon();
        }
        ShowingPurchasableHarvestTiles = false;
    }

    private void HandleTurn()
    {
        turnTimer++;
        if (hudElements != null)
        {
            hudElements.UpdateDrawSlider(turnTimer);
        }
        if (turnTimer > turnThreshold)
        {
            turn.Invoke();
            turnTimer = 0;
        }

    }

    private void TriggerAllCreatureAbilities()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> kp in tilesOwned)
        {
            if (kp.Value.CreatureOnTile() != null && kp.Value.CreatureOnTile().playerOwningCreature == this)
            {
                kp.Value.CreatureOnTile().Garrison();
            }
        }
    }

    private void HandleDrawCards()
    {
        DrawCard();
    }

    private void HandleMana()
    {
        AddToMana();
    }

    private bool CheckToSeeIfClickedHarvestTileCanBePurchased(Vector3Int tilePositionSent)
    {
        if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent)))
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent))
            {
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent).harvestCost == 0) return false;
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent).playerOwningTile == this && BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent).harvestCost <= totalMana)
                {
                    return true;
                }
            }
        }
        return false;
    }

    #region regionOfTicks
    void AddToTickQueueLocal(Vector3Int positionSent)
    {
        locallySelectedCreature = null;
        tempLocalPositionsToSend.Add(positionSent);
    }
    private void AddToPuchaseTileQueueLocal(Vector3Int cellPositionSentToClients)
    {
        tempLocalTilePositionPurchased.Add(cellPositionSentToClients);
    }
    void AddIndexOfCardInHandToTickQueueLocal(int index)
    {
        tempLocalIndecesOfCardsInHand.Add(index);
    }

    List<int> tempIndexOfCreatureOnBoard = new List<int>();
    void AddIndexOfCreatureOnBoard(int index)
    {
        tempIndexOfCreatureOnBoard.Add(index);
    }
    void AddToTickQueue(Vector3Int positionSent)
    {
        clickQueueForTick.Add(positionSent);
    }
    List<int> indecesOfCreaturesInQueue = new List<int>();
    void AddToCreatureOnBoardQueue(int index)
    {
        indecesOfCreaturesInQueue.Add(index);
    }
    void AddToIndexQueue(int indexSent)
    {
        IndecesOfCardsInHandQueue.Add(indexSent);
    }

    void SendAllInputsInQueue()
    {
        Message message = new Message();
        message.leftClicksWorldPos = tempLocalPositionsToSend;
        message.guidsForCards = tempLocalIndecesOfCardsInHand;
        message.guidsForCreatures = tempIndexOfCreatureOnBoard;
        message.localTilePositionsToBePurchased = tempLocalTilePositionPurchased;
        //message.timeBetweenLastTick = timeBetweenLastTick;
        //set guids of struct
        string messageString = JsonUtility.ToJson(message);
        SendMessageServerRpc(messageString);
        for (int i = 0; i < tempLocalTilePositionPurchased.Count; i++)
        {
            localTilePositionPurchasedToSend.Add(tempLocalTilePositionPurchased[i]);
        }
        for (int i = 0; i < tempLocalPositionsToSend.Count; i++)
        {
            clickQueueForTick.Add(tempLocalPositionsToSend[i]);
        }
        for (int i = 0; i < tempLocalIndecesOfCardsInHand.Count; i++)
        {
            IndecesOfCardsInHandQueue.Add(tempLocalIndecesOfCardsInHand[i]);
        }
        for (int i = 0; i < tempIndexOfCreatureOnBoard.Count; i++)
        {
            indecesOfCreaturesInQueue.Add(tempIndexOfCreatureOnBoard[i]);
        }
        tempLocalPositionsToSend.Clear();
        tempLocalIndecesOfCardsInHand.Clear();
        tempIndexOfCreatureOnBoard.Clear();
        tempLocalTilePositionPurchased.Clear();
        if (!GameManager.singleton.playersThatHaveBeenReceived.Contains(this))
        {
            GameManager.singleton.AddToPlayersThatHaveBeenReceived(this);
        }
    }

    void TranslateToFuntionalStruct(string jsonOfMessage)
    {
        Message receievedMessage = JsonUtility.FromJson<Message>(jsonOfMessage);

        //timeBetweenLastTick = receievedMessage.timeBetweenLastTick;
        if (receievedMessage.localTilePositionsToBePurchased.Count > 0)
        {
            for (int i = 0; i < receievedMessage.localTilePositionsToBePurchased.Count; i++)
            {
                localTilePositionPurchasedToSend.Add(receievedMessage.localTilePositionsToBePurchased[i]);
            }
        }
        if (receievedMessage.guidsForCards.Count > 0)
        {
            for (int i = 0; i < receievedMessage.guidsForCards.Count; i++)
            {
                AddToIndexQueue(receievedMessage.guidsForCards[i]);
            }
        }
        if (receievedMessage.guidsForCreatures.Count > 0)
        {
            for (int i = 0; i < receievedMessage.guidsForCreatures.Count; i++)
            {
                AddToCreatureOnBoardQueue(receievedMessage.guidsForCreatures[i]);
            }
        }
        if (receievedMessage.leftClicksWorldPos.Count > 0)
        {
            for (int i = 0; i < receievedMessage.leftClicksWorldPos.Count; i++)
            {
                AddToTickQueue(receievedMessage.leftClicksWorldPos[i]);
            }
        }
        if (!GameManager.singleton.playersThatHaveBeenReceived.Contains(this))
        {
            GameManager.singleton.AddToPlayersThatHaveBeenReceived(this);
        }
    }
    void OnTick()
    {
        tick++;
        //order matters here bigtime later set this up in the enum

        for (int i = 0; i < localTilePositionPurchasedToSend.Count; i++)
        {
            PurchaseHarvestTile(localTilePositionPurchasedToSend[i]);
        }
        for (int i = 0; i < IndecesOfCardsInHandQueue.Count; i++)
        {
            LocalSelectCardWithIndex(IndecesOfCardsInHandQueue[i]);
        }
        for (int i = 0; i < indecesOfCreaturesInQueue.Count; i++)
        {
            SetToCreatureOnFieldSelected(GameManager.singleton.allCreaturesOnField[indecesOfCreaturesInQueue[i]]);
        }
        for (int i = 0; i < clickQueueForTick.Count; i++)
        {
            LocalLeftClick(clickQueueForTick[i]);
        }
        localTilePositionPurchasedToSend.Clear();
        clickQueueForTick.Clear();
        IndecesOfCardsInHandQueue.Clear();
        indecesOfCreaturesInQueue.Clear();
        hasTickedSinceSendingLastMessage = true;
    }

    private void PurchaseHarvestTile(Vector3Int vector3Int)
    {
        SubtractFromMana(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int).harvestCost);
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int));
    }

    #endregion

    void LocalLeftClick(Vector3Int positionSent)
    {
        switch (state)
        {
            case State.PlacingCastle:
                LocalPlaceCastle(positionSent);
                break;
            case State.NothingSelected:
                break;
            case State.CreatureInHandSelected:
                HandleCreatureInHandSelected(positionSent);
                break;
            case State.SpellInHandSelected:
                HandleSpellInHandSelected(positionSent);
                break;
            case State.CreatureSelected:
                HandleCreatureOnBoardSelected(positionSent);
                break;
        }
    }

    void LocalPlaceCastle(Vector3Int positionSent)
    {
        placedCellPosition = positionSent;
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).traverseType == BaseTile.traversableType.Untraversable)
        {
            return;
        }
        Vector3 positionToSpawn = highlightMap.GetCellCenterWorld(placedCellPosition);

        SetOwningTile(placedCellPosition);

        for (int i = 0; i < BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition).neighborTiles.Count; i++)
        {
            SetOwningTile(BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition).neighborTiles[i].tilePosition);
        }
        instantiatedCaste = Instantiate(castle, positionToSpawn, Quaternion.identity);
        instantiatedCaste.GetComponent<MeshRenderer>().material.color = col;
        instantiatedCaste.GetComponent<Structure>().playerOwningStructure = this;
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition));
        AddToMana();
        SetStateToWaiting();
        GameManager.singleton.AddPlayerToReady(this);
    }

    private void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        harvestedTiles.Add(baseTileSent);
        baseTileSent.SetBeingHarvested();
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (!harvestedTiles.Contains(bt.Value))
            {
                bt.Value.SetHarvestCost(harvestedTiles.Count);
            }
        }
        //AddToMaxMana(baseTileSent.manaType);
    }

    bool CheckForRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHitCardInHand, Mathf.Infinity))
        {
            if (raycastHitCardInHand.transform.GetComponent<CardInHand>() != null)
            {
                if (raycastHitCardInHand.transform.GetComponent<CardInHand>().isPurchasable)
                {
                    AddIndexOfCardInHandToTickQueueLocal(raycastHitCardInHand.transform.GetComponent<CardInHand>().indexOfCard);
                    return true;
                }
            }
        }
        if (ShowingPurchasableHarvestTiles && CheckToSeeIfClickedHarvestTileCanBePurchased(currentLocalHoverCellPosition))
        {
            return false;
        }
        if (Physics.Raycast(ray, out RaycastHit raycastHitCreatureOnBoard, Mathf.Infinity, creatureMask))
        {
            if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>() != null)
            {
                if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>().playerOwningCreature == this)
                {
                    locallySelectedCreature = raycastHitCreatureOnBoard.transform.GetComponent<Creature>();
                    AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);

                    //SetToCreatureOnFieldSelected(raycastHitCreatureOnBoard.transform.GetComponent<Creature>());
                    return true;
                }
                if (state == State.SpellInHandSelected)
                {
                    AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                    return true;
                }
            }
        }
        return false;
    }

    private void VisualPathfinderOnCreatureSelected(Creature creature)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).traverseType == BaseTile.traversableType.Untraversable || creature.thisTraversableType == Creature.travType.Walking && BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).traverseType != BaseTile.traversableType.TraversableByAll)
        {
            creature.HidePathfinderLR();
            return;
        }
        creature.ShowPathfinderLinerRendererAsync(currentLocalHoverCellPosition);
    }

    void LocalSelectCardWithIndex(int indexOfCardSelected)
    {
        CardInHand cardToSelect;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].indexOfCard == indexOfCardSelected)
            {
                cardToSelect = cardsInHand[i];
                cardSelected = cardToSelect;
                if (cardSelected.cardType == CardInHand.CardType.Creature)
                {
                    state = State.CreatureInHandSelected;
                }
                if (cardSelected.cardType == CardInHand.CardType.Spell)
                {
                    state = State.SpellInHandSelected;
                }
            }
        }

    }
    void HandleCreatureOnBoardSelected(Vector3Int positionSent)
    {
        targetedCellPosition = positionSent;
        #region creatureSelected
        if (creatureSelected != null)
        {
            creatureSelected.SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetedCellPosition));

            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition) == creatureSelected.tileCurrentlyOn) //this makes sure you can double click to stop the creature and also have it selected
            {
                SetToCreatureOnFieldSelected(creatureSelected);
                return;
            }
            creatureSelected = null;
            SetStateToNothingSelected();
        }
        #endregion
    }

    Dictionary<int, Creature> creaturesOwned = new Dictionary<int, Creature>();
    void HandleCreatureInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            //show error
            return;
        }

        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).playerOwningTile == this)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).structureOnTile == null)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
                if (environmentMap.GetInstantiatedObject(cellSent))
                {
                    GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
                    if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
                    {
                        instantiatedObject.AddComponent<ChangeTransparency>();
                    }
                    ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
                    instantiatedObjectsChangeTransparency.ChangeTransparent(100);
                }
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedCreature = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);
                creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>().ownedCreatureID, instantiatedCreature.GetComponent<Creature>());
                RemoveCardFromHand(cardSelected);
                SetStateToNothingSelected();
            }
        }
    }

    private void HandleSpellInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range != 0)
        {
            Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
            if (environmentMap.GetInstantiatedObject(cellSent))
            {
                GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
                if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
                {
                    instantiatedObject.AddComponent<ChangeTransparency>();
                }
                ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
                instantiatedObjectsChangeTransparency.ChangeTransparent(100);
            }

            SpendManaToCast(cardSelected.GetComponent<CardInHand>());
            GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
            instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
            RemoveCardFromHand(cardSelected);
            SetStateToNothingSelected();
        }
    }

    private void SpendManaToCast(CardInHand cardSelected)
    {
        resources.blueMana -= cardSelected.blueManaCost;
        resources.redMana -= cardSelected.redManaCost;
        resources.whiteMana -= cardSelected.whiteManaCost;
        resources.blackMana -= cardSelected.blackManaCost;
        resources.greenMana -= cardSelected.greenManaCost;
        resourcesChanged.Invoke(resources);
    }

    void SetOwningTile(Vector3Int cellPosition)
    {
        if (!tilesOwned.ContainsKey(cellPosition))
        {
            tilesOwned.Add(cellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition));
            BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition).SetOwnedByPlayer(this);
        }
    }

    void DrawCard()
    {
        if (cardsInDeck.Count <= 0)
        {
            return;
        }
        if (cardsInHand.Count >= maxHandSize)
        {
            return;
        }
        CardInHand cardAddingToHand = cardsInDeck[cardsInDeck.Count - 1]; //todo this might cause problems when dealing with shuffling cards back into the deck
        cardAddingToHand.indexOfCard = cardsInDeck.Count - 1;
        cardsInDeck.RemoveAt(cardsInDeck.Count - 1);

        GameObject instantiatedCardInHand = Instantiate(cardAddingToHand.gameObject, cardParent);
        CardInHand instantiatedCardInHandBehaviour = instantiatedCardInHand.GetComponent<CardInHand>();
        instantiatedCardInHandBehaviour.indexOfCard = cardAddingToHand.indexOfCard;

        cardsInHand.Add(instantiatedCardInHandBehaviour);
        instantiatedCardInHandBehaviour.playerOwningCard = this;
        instantiatedCardInHandBehaviour.CheckToSeeIfPurchasable(resources);
    }

    void RemoveCardFromHand(CardInHand cardToRemove)
    {
        cardsInHand.Remove(cardToRemove);
        Destroy(cardToRemove.gameObject);
    }


    int indexOfCardInHandSelected;
    public void SetToCreatureOnFieldSelected(Creature creatureSelectedSent)
    {
        if (state == State.SpellInHandSelected)
        {
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0)
            {
                Debug.Log("Casting spell on " + creatureSelectedSent);
                CastSpellOnTargetedCreature(creatureSelectedSent);
            }
            return;
        }
        creatureSelected = creatureSelectedSent;
        state = State.CreatureSelected;
    }

    private void CastSpellOnTargetedCreature(Creature creatureSelectedSent)
    {
        SetStateToNothingSelected();
    }

    void SetStateToNothingSelected()
    {
        cardSelected = null;
        creatureSelected = null;
        state = State.NothingSelected;
    }
    void SetStateToWaiting()
    {
        cardSelected = null;
        creatureSelected = null;
        state = State.Waiting;
    }


    private void SubtractFromMana(int harvestCost)
    {
        int totalAdded = 0;
        for (int x = 0; x < harvestCost; x++)
        {
            if (totalAdded >= harvestCost) continue;
            if (resources.blueMana > 0)
            {
                for (int i = 0; i < resources.blueMana; i++)
                {
                    totalAdded++;
                    resources.blueMana--;
                    if (totalAdded == harvestCost) continue;
                }
            }
            if (resources.whiteMana > 0)
            {
                for (int i = 0; i < resources.whiteMana; i++)
                {
                    totalAdded++;
                    resources.whiteMana--;
                    if (totalAdded == harvestCost) continue;
                }
            }
            if (resources.redMana > 0)
            {
                for (int i = 0; i < resources.redMana; i++)
                {
                    totalAdded++;
                    resources.redMana--;
                    if (totalAdded == harvestCost) continue;
                }
            }
            if (resources.blackMana > 0)
            {
                for (int i = 0; i < resources.blackMana; i++)
                {
                    totalAdded++;
                    resources.blackMana--;
                    if (totalAdded == harvestCost) continue;
                }
            }
            if (resources.greenMana > 0)
            {
                for (int i = 0; i < resources.greenMana; i++)
                {
                    totalAdded++;
                    resources.greenMana--;
                    if (totalAdded == harvestCost) continue;
                }
            }
        }
        resourcesChanged.Invoke(resources);
    }
    public void AddToMana()
    {
        for (int i = 0; i < harvestedTiles.Count; i++)
        {
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Blue)
            {
                resources.blueMana++;
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Black)
            {
                resources.blackMana++;
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Red)
            {
                resources.redMana++;
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.White)
            {
                resources.whiteMana++;
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Green)
            {
                resources.greenMana++;
            }
        }

        resourcesChanged.Invoke(resources);
    }
    public void AddSpecificManaToPool(BaseTile.ManaType manaTypeSent)
    {
        if (manaTypeSent == BaseTile.ManaType.Black)
        {
            resources.blackMana++;
        }
        if (manaTypeSent == BaseTile.ManaType.Blue)
        {
            resources.blueMana++;
        }
        if (manaTypeSent == BaseTile.ManaType.Red)
        {
            resources.redMana++;
        }
        if (manaTypeSent == BaseTile.ManaType.Green)
        {
            resources.greenMana++;
        }
        if (manaTypeSent == BaseTile.ManaType.White)
        {
            resources.whiteMana++;
        }

        resourcesChanged.Invoke(resources);
    }
    int totalMana;
    private void UpdateHudForResourcesChanged(PlayerResources resources)
    {
        if (IsOwner)
        {
            hudElements.UpdateHudElements(resources);
        }
        foreach (CardInHand cardInHand in cardsInHand)
        {
            cardInHand.CheckToSeeIfPurchasable(resources);
        }
        totalMana = resources.blackMana + resources.blueMana + resources.whiteMana + resources.greenMana + resources.redMana;
    }




    #region RPCS
    [ServerRpc]
    private void SendMessageServerRpc(string json)
    {
        SendMessageClientRpc(json);
    }
    [ClientRpc]
    private void SendMessageClientRpc(string json)
    {
        if (IsOwner) return;
        TranslateToFuntionalStruct(json);
    }
    #endregion



}

public struct PlayerResources
{
    public int blueMana;
    public int redMana;
    public int whiteMana;
    public int blackMana;
    public int greenMana;

}
