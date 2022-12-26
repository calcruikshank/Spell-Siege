using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        //CreatureSelected,
        CreatureInHandSelected,
        SpellInHandSelected,
        StructureInHandSeleced,
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
    public Creature creatureSelected;
    Vector3Int currentLocalHoverCellPosition;
    Vector3Int cellPositionSentToClients;
    Vector3Int targetedCellPosition;

    [SerializeField] LayerMask creatureMask;

    Vector3Int placedCellPosition;

    public int turnTimer;
    int turnThreshold = 80; //todo make this 800
    int maxHandSize = 7;
    [SerializeField] List<CardInHand> dragonDeck = new List<CardInHand>();
    [SerializeField] List<CardInHand> demonDeck = new List<CardInHand>();
    public List<CardInHand> cardsInDeck = new List<CardInHand>();
    public List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardInHand cardSelected;
    public CardInHand locallySelectedCard;
    public List<Vector3> allVertextPointsInTilesOwned = new List<Vector3>();

    Transform instantiatedPlayerUI;
    Transform cardParent;

    Canvas canvasMain;

    public int tick = 0; //this is for determining basically everything
    public float tickTimer = 0f;
    float tickThreshold = .12f;

    public bool hasTickedSinceSendingLastMessage = true;
    public Creature locallySelectedCreature;

    PlayerResources resources;

    delegate void ResourcesChanged(PlayerResources resources);
    ResourcesChanged resourcesChanged;

    [SerializeField] Transform playerHud;
    HudElements hudElements;
    delegate void Turn();
    event Turn turn;

    int numOfPurchasableHarvestTiles = 1;

    public List<BaseTile> harvestedTiles = new List<BaseTile>();


    public int spellCounter = 0;

    public List<ActionStruct> finalOrder = new List<ActionStruct>();

    public List<ActionStruct> localOrder = new List<ActionStruct>();

    public bool creaturePathLockedIn = false;

    [Serializable]
    public enum ActionTaken
    {
        LeftClickBaseMap,
        SelectedCreature,
        SelectedCardInHand,
        RightClick,
        TilePurchased
    }

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
        cardsInDeck = new List<CardInHand>();

        if (NetworkManager.Singleton.IsHost && IsOwner)
        {
            cardsInDeck = dragonDeck;
        }
        if (!IsHost && IsOwner)
        {
            cardsInDeck = demonDeck;
        }

        if (!IsOwner && IsHost)
        {
            cardsInDeck = demonDeck;
        }
        if (!IsOwner && !IsHost)
        {
            cardsInDeck = dragonDeck;
        }
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
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureInHandSelected:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.SpellInHandSelected:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.StructureInHandSeleced:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
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
            if (locallySelectedCreature != null && !creaturePathLockedIn)
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
        if (locallySelectedCard != null)
        {
            if (locallySelectedCard.GetComponentInChildren<BoxCollider>().enabled == true)
            {
                locallySelectedCard.GetComponentInChildren<BoxCollider>().enabled = false;
                locallySelectedCard.gameObject.SetActive(true);
                foreach (Image img in locallySelectedCard.GetComponentsInChildren<Image>())
                {
                    Color imageColor = img.color;
                    imageColor.a = .4f;
                    img.color = imageColor;
                }
                foreach (TextMeshProUGUI tmp in locallySelectedCard.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    Color imageColor = tmp.color;
                    imageColor.a = .4f;
                    tmp.color = imageColor;
                }
            }
            locallySelectedCard.transform.position = new Vector3(mousePosition.x, mousePosition.y + 1f, mousePosition.z);
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (instantiatedCaste != null)
            {
                SetVisualsToNothingSelectedLocally();
                localOrder.Add(new ActionStruct(ActionTaken.RightClick, true));
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (locallySelectedCreature != null || locallySelectedCard != null)
            {
                if (cellPositionSentToClients != null)
                {
                    if (cellPositionSentToClients != grid.WorldToCell(mousePosition))
                    {
                        if (!CheckForRaycast())
                        {
                            AddToTickQueueLocal(grid.WorldToCell(mousePosition));
                        }
                        if (locallySelectedCreature != null)
                        {
                            LockInVisualPathfinder();
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (locallySelectedCreature != null)
            {
                LockInVisualPathfinder();
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePositionWorldPoint;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, mousePositionScript.baseTileMap))
            {
                mousePositionWorldPoint = raycastHit.point;
                cellPositionSentToClients = grid.WorldToCell(mousePositionWorldPoint);
            }

            if (state == State.NothingSelected)
            {
                if (!CheckForRaycast())
                {
                    AddToTickQueueLocal(cellPositionSentToClients);
                    if (locallySelectedCard != null)
                    {
                        Destroy(locallySelectedCard.gameObject);
                    }
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

    private void LockInVisualPathfinder()
    {
        creaturePathLockedIn = true;
    }

    public void SetVisualsToNothingSelectedLocally()
    {
        if (locallySelectedCreature != null)
        {
            locallySelectedCreature.HidePathfinderLR();
            locallySelectedCreature = null;
        }

        if (locallySelectedCard != null)
        {
            if (locallySelectedCardInHandToTurnOff != null)
            {
                locallySelectedCardInHandToTurnOff.gameObject.SetActive(true);
            }
            Destroy(locallySelectedCard.gameObject);
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
            case State.StructureInHandSeleced:
                HandleTurn();
                break;
        }
    }

    public bool ShowingPurchasableHarvestTiles = false;

    bool keepClicked = false;
    private void HandleSpacebarPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            keepClicked = false;
            CameraControl.Singleton.ReturnHome(new Vector3(instantiatedCaste.transform.position.x, instantiatedCaste.transform.position.y, instantiatedCaste.transform.position.z - 7));
            ShowHarvestedTiles();
        }
        if (!keepClicked)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                CameraControl.Singleton.CancelReturnHome();
                HideHarvestedTiles();
            }
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
        foreach (KeyValuePair<int, Creature> kp in creaturesOwned)
        {
            kp.Value.OnTurn();
        }
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
        Vector3 positionOfCreature = new Vector3();
        if (locallySelectedCreature && IsOwner)
        {
            positionOfCreature = locallySelectedCreature.actualPosition;

            creatureSelected = locallySelectedCreature;
            HandleCreatureOnBoardSelected(positionSent, positionOfCreature);
        }
        localOrder.Add(new ActionStruct(ActionTaken.LeftClickBaseMap, positionSent, positionOfCreature));
    }
    private void AddToPuchaseTileQueueLocal(Vector3Int cellPositionSentToClients)
    {
        localOrder.Add(new ActionStruct(ActionTaken.TilePurchased, cellPositionSentToClients, Vector3.zero));
    }
    void AddIndexOfCardInHandToTickQueueLocal(int index)
    {
        localOrder.Add(new ActionStruct(ActionTaken.SelectedCardInHand, index));
    }
    void AddIndexOfCreatureOnBoard(int index)
    {
        localOrder.Add(new ActionStruct(ActionTaken.SelectedCreature, index));
    }

    void SendAllInputsInQueue()
    {
        Message message = new Message();
        //message.timeBetweenLastTick = timeBetweenLastTick;
        //set guids of struct

        if (localOrder.Count > 0)
        {
            message.ActionsInOrder = localOrder;
        }


        string messageString = JsonUtility.ToJson(message);

        SendMessageServerRpc(messageString);

        for (int i = 0; i < localOrder.Count; i++)
        {
            finalOrder.Add(localOrder[i]);
        }

        if (!GameManager.singleton.playersThatHaveBeenReceived.Contains(this))
        {
            GameManager.singleton.AddToPlayersThatHaveBeenReceived(this);
        }
        localOrder.Clear();
    }

    void TranslateToFuntionalStruct(string jsonOfMessage)
    {
        Message receievedMessage = JsonUtility.FromJson<Message>(jsonOfMessage);


        foreach (ActionStruct c in receievedMessage.ActionsInOrder)
        {
            finalOrder.Add(c);
        }



        if (!GameManager.singleton.playersThatHaveBeenReceived.Contains(this))
        {
            GameManager.singleton.AddToPlayersThatHaveBeenReceived(this);
        }
    }
    void OnTick()
    {
        tick++;

        for (int i = 0; i < finalOrder.Count; i++)
        {
            ActionStruct actionGrabbed = finalOrder[i];

            if (actionGrabbed.actionType == ActionTaken.LeftClickBaseMap)
            {
                LocalLeftClick(actionGrabbed.actionInputVector, actionGrabbed.positionOfCreature);
            }
            if (actionGrabbed.actionType == ActionTaken.RightClick)
            {
                SetStateToNothingSelected();
            }
            if (actionGrabbed.actionType == ActionTaken.SelectedCardInHand)
            {
                LocalSelectCardWithIndex((int)actionGrabbed.actionInputInt);
            }
            if (actionGrabbed.actionType == ActionTaken.SelectedCreature)
            {
                SetToCreatureOnFieldSelected(GameManager.singleton.allCreaturesOnField[(int)actionGrabbed.actionInputInt]);
            }
            if (actionGrabbed.actionType == ActionTaken.TilePurchased)
            {
                PurchaseHarvestTile((Vector3Int)actionGrabbed.actionInputVector);
            }
        }
        finalOrder.Clear();
        hasTickedSinceSendingLastMessage = true;
    }

    private void PurchaseHarvestTile(Vector3Int vector3Int)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int).isBeingHarvested)
        {
            return;
        }
        SpendGenericMana(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int).harvestCost);
        manaCap++;
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int));
        IncreaseCostOfHarvestTiles();
    }

    int harvestCost = 3;
    private void IncreaseCostOfHarvestTiles()
    {
        harvestCost = harvestedTiles.Count * 3;

        //AddToMaxMana(baseTileSent.manaType);
    }

    #endregion

    void LocalLeftClick(Vector3Int positionSent, Vector3 positionOfCreatureSent)
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
            case State.StructureInHandSeleced:
                HandleStructureInHandSelected(positionSent);
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
        AddStructureToTile(instantiatedCaste.GetComponent<Structure>(), positionSent);
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition));
        IncreaseCostOfHarvestTiles();
        AddToMana();
        SetStateToWaiting();
        GameManager.singleton.AddPlayerToReady(this);
    }

    private void AddStructureToTile(Structure structure, Vector3Int positionSent)
    {
        BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).structureOnTile = structure;
        structure.tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent);
        structure.playerOwningStructure = this;
    }

    private void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        harvestedTiles.Add(baseTileSent);
        baseTileSent.SetBeingHarvested();
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (!harvestedTiles.Contains(bt.Value))
            {
                bt.Value.SetHarvestCost(harvestCost);
            }
        }
    }

    CardInHand locallySelectedCardInHandToTurnOff;
    bool CheckForRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHitKeep, Mathf.Infinity))
        {
            if (raycastHitKeep.transform.GetComponent<PlayerKeep>() != null)
            {
                if (raycastHitKeep.transform.GetComponent<PlayerKeep>().playerOwningStructure == this)
                {
                    if (locallySelectedCard == null || locallySelectedCard.cardType != CardInHand.CardType.Spell)
                    {
                        if (!keepClicked)
                        {
                            keepClicked = true;
                            ShowHarvestedTiles();
                            return true;
                        }
                        if (keepClicked)
                        {
                            keepClicked = false;
                            HideHarvestedTiles();
                            return true;
                        }
                    }
                }
            }
        }
        if (Physics.Raycast(ray, out RaycastHit raycastHitCardInHand, Mathf.Infinity))
        {
            if (raycastHitCardInHand.transform.GetComponent<CardInHand>() != null)
            {
                if (raycastHitCardInHand.transform.GetComponent<CardInHand>().isPurchasable)
                {
                    locallySelectedCardInHandToTurnOff = raycastHitCardInHand.transform.GetComponent<CardInHand>();
                    locallySelectedCardInHandToTurnOff.TurnOffVisualCard();
                    locallySelectedCard = Instantiate(raycastHitCardInHand.transform.GetComponent<CardInHand>().gameObject, canvasMain.transform).GetComponent<CardInHand>();
                    locallySelectedCard.transform.position = locallySelectedCardInHandToTurnOff.transform.position;
                    locallySelectedCard.transform.localEulerAngles = Vector3.zero;
                    raycastHitCardInHand.transform.GetComponent<CardInHand>().gameObject.SetActive(false);
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
                if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>().playerOwningCreature == this && state != State.SpellInHandSelected && locallySelectedCreature == null)
                {
                    locallySelectedCreature = raycastHitCreatureOnBoard.transform.GetComponent<Creature>();
                    creaturePathLockedIn = false;
                    AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                    return true;
                }
                if (state == State.SpellInHandSelected || state == State.StructureInHandSeleced)
                {
                    if (cardSelected.GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
                    {
                        AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                        return true;
                    }
                }
                if (locallySelectedCreature != null)
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
        if (creaturePathLockedIn) return;
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).traverseType == BaseTile.traversableType.Untraversable || creature.thisTraversableType == Creature.travType.Walking && BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).traverseType != BaseTile.traversableType.TraversableByAll)
        {
            creature.HidePathfinderLR();
            return;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).structureOnTile != null)
        {
            //creature.HidePathfinderLR();
            //return;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).CreatureOnTile() != null)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentLocalHoverCellPosition).CreatureOnTile() == this)
            {
                creature.HidePathfinderLR();
                return;
            }
            //creature.HidePathfinderLR();
            //return;
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
                if (cardSelected.cardType == CardInHand.CardType.Structure)
                {
                    state = State.StructureInHandSeleced;
                }
            }
        }

    }
    void HandleCreatureOnBoardSelected(Vector3Int positionSent, Vector3 positionOfCreatureSent)
    {
        MoveCreatureServerRpc(positionSent, positionOfCreatureSent, creatureSelected.creatureID);
        targetedCellPosition = positionSent;
        #region creatureSelected
        if (creatureSelected != null)
        {
            creatureSelected.targetToFollow = null;
            creatureSelected.structureToFollow = null;
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).structureOnTile != null)
            {
                creatureSelected.SetStructureToFollow(BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).structureOnTile);
                SetVisualsToNothingSelectedLocally();
            }
            else
            {
                SetVisualsToNothingSelectedLocally();
                creatureSelected.SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetedCellPosition), positionOfCreatureSent);

            }
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

    [ServerRpc]
    private void MoveCreatureServerRpc(Vector3Int positionSent, Vector3 positionOfCreatureSent, int creatureID)
    {
        MoveCreatureClientRpc(positionSent, positionOfCreatureSent, creatureID);
    }
    [ClientRpc]
    private void MoveCreatureClientRpc(Vector3Int positionSent, Vector3 positionOfCreatureSent, int creatureID)
    {
        MoveNonOwnedCreature(positionSent, positionOfCreatureSent, creatureID);
    }

    public void MoveNonOwnedCreature(Vector3Int positionSent, Vector3 positionOfCreatureSent, int creatureID)
    {
        creatureSelected = GameManager.singleton.allCreaturesOnField[creatureID];
        if (!IsOwner)
        {
            targetedCellPosition = positionSent;
            #region creatureSelected
            if (creatureSelected != null)
            {
                creatureSelected.targetToFollow = null;
                creatureSelected.structureToFollow = null;
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).structureOnTile != null)
                {
                    creatureSelected.SetStructureToFollow(BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).structureOnTile);
                    SetVisualsToNothingSelectedLocally();
                }
                else
                {
                    SetVisualsToNothingSelectedLocally();
                    creatureSelected.SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetedCellPosition), positionOfCreatureSent);

                    for (int i = 0; i < 20; i++)
                    {
                        creatureSelected.Move();
                    }
                }
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition) == creatureSelected.tileCurrentlyOn) //this makes sure you can double click to stop the creature and also have it selected
                {
                    SetToCreatureOnFieldSelected(creatureSelected);
                    return;
                }
                creatureSelected = null;
                SetStateToNothingSelected();
            }
        }
        #endregion
    }


    public Dictionary<int, Creature> creaturesOwned = new Dictionary<int, Creature>();
    void HandleCreatureInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            //show error
            return;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == BaseTile.traversableType.SwimmingAndFlying && cardSelected.GameObjectToInstantiate.GetComponent<Creature>().thisTraversableType == Creature.travType.Walking)
        {
            return;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).playerOwningTile == this)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).structureOnTile == null && BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).CreatureOnTile() == null)
            {

                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                CastCreatureOnTile(cardSelected, cellSent);
            }
        }
    }

    public void CastCreatureOnTile(CardInHand cardSelectedSent, Vector3Int cellSent)
    {
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
        GameObject instantiatedCreature = Instantiate(cardSelectedSent.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
        RemoveCardFromHand(cardSelectedSent);
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

        instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);

        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent);
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>().creatureID, instantiatedCreature.GetComponent<Creature>());
        cardSelectedSent.transform.parent = null;
        SetStateToNothingSelected();
    }

    private void HandleSpellInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>() != null)
        {
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

                RemoveCardFromHand(cardSelected);
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetStateToNothingSelected();
                return;
            }
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                RemoveCardFromHand(cardSelected);
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetStateToNothingSelected();
                return;
            }
        }
    }
    private void HandleStructureInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (cardSelected != null)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                if (cardSelected != null)
                {
                    if (kvp.Value.neighborTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
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

                            Destroy(instantiatedObject);
                        }

                        SetOwningTile(cellSent);

                        SpendManaToCast(cardSelected.GetComponent<CardInHand>()); //she works out too much 
                        GameObject instantiatedStructure = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                        instantiatedStructure.GetComponent<Structure>().InjectDependencies(cellSent, this);

                        foreach (BaseTile bt in BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).neighborTiles)
                        {
                            SetOwningTile(bt.tilePosition);
                        }
                        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent));
                        manaCap++;
                        RemoveCardFromHand(cardSelected);
                        SetStateToNothingSelected();
                        return;
                    }
                }
            }
        }

    }

    private void SpendManaToCast(CardInHand cardSelected)
    {
        resources.blueMana -= cardSelected.blueManaCost;
        resources.redMana -= cardSelected.redManaCost;
        resources.whiteMana -= cardSelected.whiteManaCost;
        resources.blackMana -= cardSelected.blackManaCost;
        resources.greenMana -= cardSelected.greenManaCost;
        SpendGenericMana(cardSelected.genericManaCost);
        resourcesChanged.Invoke(resources);
    }

    private void SpendGenericMana(int genericManaCost)
    {
        int totalManaSpent = 0;
        for (int i = 0; i < genericManaCost; i++)
        {
            if (resources.blackMana > 0)
            {
                resources.blackMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.blueMana > 0)
            {
                resources.blueMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.redMana > 0)
            {
                resources.redMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.whiteMana > 0)
            {
                resources.whiteMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.greenMana > 0)
            {
                resources.greenMana--;
                totalManaSpent++;
                continue;
            }
        }
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

    public void DrawCard()
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
    public void DrawCardWithIndex(int indexOfCard)
    {
        if (cardsInDeck.Count <= 0)
        {
            return;
        }
        if (cardsInHand.Count >= maxHandSize)
        {
            return;
        }
        CardInHand cardAddingToHand = cardsInDeck[indexOfCard]; //todo this might cause problems when dealing with shuffling cards back into the deck
        cardAddingToHand.indexOfCard = cardsInDeck.Count - 1;
        cardsInDeck.RemoveAt(indexOfCard);

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
        if (cardToRemove != null)
        {
            if (cardToRemove.cardType != CardInHand.CardType.Creature)
            {
                cardToRemove.transform.parent = null;
            }
        }
    }


    int indexOfCardInHandSelected;
    public void SetToCreatureOnFieldSelected(Creature creatureSelectedSent)
    {
        if (state == State.SpellInHandSelected)
        {
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
            {
                Debug.Log("Casting spell on " + creatureSelectedSent);
                CastSpellOnTargetedCreature(creatureSelectedSent);
            }
            return;
        }
        creatureSelected = creatureSelectedSent;
    }

    private void CastSpellOnTargetedCreature(Creature creatureSelectedSent)
    {
        SpendManaToCast(cardSelected.GetComponent<CardInHand>());
        GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, creatureSelectedSent.tileCurrentlyOn.tilePosition, Quaternion.identity);
        instantiatedSpell.GetComponent<TargetedSpell>().InjectDependencies(creatureSelectedSent, this);
        RemoveCardFromHand(cardSelected);
        OnSpellCast();

        SetStateToNothingSelected();
    }

    public void SetStateToNothingSelected()
    {
        if (locallySelectedCard != null)
        {
            if (locallySelectedCardInHandToTurnOff != null)
            {
                locallySelectedCardInHandToTurnOff.gameObject.SetActive(true);
            }
            Destroy(locallySelectedCard.gameObject);
        }
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


    int manaCap = 5;
    public void AddToMana()
    {
        for (int i = 0; i < harvestedTiles.Count; i++)
        {
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Blue)
            {
                if (resources.blueMana < manaCap)
                {
                    resources.blueMana++;
                }
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Black)
            {
                if (resources.blackMana < manaCap)
                {
                    resources.blackMana++;
                }
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Red)
            {
                if (resources.redMana < manaCap)
                {
                    resources.redMana++;
                }
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.White)
            {
                if (resources.whiteMana < manaCap)
                {
                    resources.whiteMana++;
                }
            }
            if (harvestedTiles[i].manaType == BaseTile.ManaType.Green)
            {
                if (resources.greenMana < manaCap)
                {
                    resources.greenMana++;
                }
            }
        }

        resourcesChanged.Invoke(resources);
    }
    public void AddSpecificManaToPool(BaseTile.ManaType manaTypeSent)
    {

        if (manaTypeSent == BaseTile.ManaType.Black)
        {
            if (resources.blackMana < manaCap)
            {
                resources.blackMana++;
            }
        }
        if (manaTypeSent == BaseTile.ManaType.Blue)
        {
            if (resources.blueMana < manaCap)
            {
                resources.blueMana++;
            }
        }
        if (manaTypeSent == BaseTile.ManaType.Red)
        {
            if (resources.redMana < manaCap)
            {
                resources.redMana++;
            }
        }
        if (manaTypeSent == BaseTile.ManaType.Green)
        {
            if (resources.greenMana < manaCap)
            {
                resources.greenMana++;
            }
        }
        if (manaTypeSent == BaseTile.ManaType.White)
        {
            if (resources.whiteMana < manaCap)
            {
                resources.whiteMana++;
            }
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



    //overridables
    public virtual void OnSpellCast()
    {
        spellCounter++;

    }

    internal CardInHand GetRandomCreatureInHand()
    {
        List<int> numbersChosen = new List<int>();
        CardInHand creatureSelectedInHand = new CardInHand();
        while (creatureSelectedInHand == null)
        {
            int randomNumber = UnityEngine.Random.Range(0, cardsInHand.Count);
            if (numbersChosen.Contains(randomNumber))
            {
                break;
            }
            numbersChosen.Add(randomNumber);
            if (cardsInHand[randomNumber].cardType == CardInHand.CardType.Creature)
            {
                creatureSelectedInHand = cardsInHand[randomNumber];
            }
        }
        return creatureSelectedInHand;
    }
}



public struct PlayerResources
{
    public int blueMana;
    public int redMana;
    public int whiteMana;
    public int blackMana;
    public int greenMana;

}

