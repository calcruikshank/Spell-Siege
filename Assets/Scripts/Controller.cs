using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Controller : NetworkBehaviour
{
    public bool isAI = false;
    public State state;
    public enum State
    {
        NothingSelected,
        CreatureInHandSelected,
        SpellInHandSelected,
        StructureInHandSeleced,
        PlacingCastle,
        SelectingDeck,
        Waiting
    }
    NetworkVariable<int> seed = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            seed.Value = UnityEngine.Random.Range(0, 2147483646);
        }
        if (IsClient)
        {
            Debug.Log(seed.Value);
            UnityEngine.Random.InitState(seed.Value);
        }
    }




    //Could use a state machine if creature is selected change state to creature selected 
    //if card in hand is selected change state to placing card
    //if neither are selected change state to selecting
    //if environment is selected change state to environment selected
    //if environment card is selected change state to environment card selected

    public Dictionary<Vector3Int, BaseTile> tilesOwned = new Dictionary<Vector3Int, BaseTile>();


    protected MousePositionScript mousePositionScript;

    public Color col;
    public Color transparentCol;


    protected Vector3 mousePosition;
    protected TileBase highlightTile;
    protected Tilemap highlightMap;
    protected Tilemap baseMap;
    protected Tilemap environmentMap;
    protected Tilemap waterMap;
    protected Grid grid;
    protected Vector3Int previousCellPosition;

    protected Transform castle;
    public PlayerKeep instantiatedCaste;
    protected Vector3Int currentLocalHoverCellPosition;
    protected Vector3Int cellPositionSentToClients;
    protected Vector3Int targetedCellPosition;

    [SerializeField] protected LayerMask creatureMask;

    protected Vector3Int placedCellPosition;

    public int turnTimer;
    protected int turnThreshold = 1400; //todo make this 1400
    protected int maxHandSize = 10;
    [SerializeField] protected List<CardInHand> dragonDeck = new List<CardInHand>();
    [SerializeField] protected List<CardInHand> demonDeck = new List<CardInHand>();
    public List<CardInHand> cardsInDeck = new List<CardInHand>();
    public List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardInHand cardSelected;
    public CardInHand locallySelectedCard;
    public List<Vector3> allVertextPointsInTilesOwned = new List<Vector3>();

    protected Transform instantiatedPlayerUI;
    protected Transform cardParent;

    protected Canvas canvasMain;


    protected PlayerResources resources;

    protected delegate void ResourcesChanged(PlayerResources resources);
    protected ResourcesChanged resourcesChanged;

    [SerializeField] protected Transform playerHud;
    protected HudElements hudElements;
    protected delegate void Turn();
    protected event Turn turn;

    protected int numOfPurchasableHarvestTiles = 1;

    public List<BaseTile> harvestedTiles = new List<BaseTile>();


    public int spellCounter = 0;


    public int numberOfLandsYouCanPlayThisTurn = 1;
    public int numberOfLandsPlayedThisTurn = 0;

    [SerializeField] protected Color[] colorsToPickFrom;


    public List<Structure> structuresOwned = new List<Structure>();

    public enum ActionTaken
    {
        LeftClickBaseMap,
        SelectedCardInHand,
        RightClick,
        TilePurchased
    }


    public Controller opponent;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        GrabAllObjectsFromGameManager();
        mousePositionScript = GetComponent<MousePositionScript>();
        if (IsHost && IsOwner)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }

        

    }
    private void OnDestroy()
    {
        if (IsHost && IsOwner)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected with ID: {clientId}");
        if (IsHost && IsOwner && NetworkManager.Singleton.ConnectedClientsList.Count >= 2 && !gameStarted)
        {
            gameStarted = true;
            StartGame();
        }
    }
    private bool gameStarted = false;
    private void StartGame()
    {
        if (!IsServer)
            return;

        Debug.Log("Starting the game as there are now 2 players connected.");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Vector3 spawnPosition = client.ClientId == NetworkManager.Singleton.LocalClientId ? serverCastlePosition : clientCastlePosition;
            Debug.Log($"Spawning castle for client {client.ClientId} at position {spawnPosition}");
            SpawnCastleForPlayer(client.ClientId, spawnPosition);
        }
        SetupCastlesServerRpc();
    }

    [SerializeField] GameObject castlePrefab;

    private void SpawnCastleForPlayer(ulong clientId, Vector3 position)
    {
        Debug.Log($"Attempting to spawn castle for client {clientId} at position {position}");

        GameObject castleInstance = Instantiate(castlePrefab, position, Quaternion.identity);
        NetworkObject networkObject = castleInstance.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component is missing from the castle prefab.");
            return;
        }

        // Assign ownership to the client
        networkObject.SpawnWithOwnership(clientId);

        // Log the owner client ID after spawning
        Debug.Log($"Castle spawned with OwnerClientId: {networkObject.OwnerClientId}");

    }


    [ServerRpc]
    void SetupCastlesServerRpc()
    {
        SetupCastlesClientRpc();
    }
    [ClientRpc]
    void SetupCastlesClientRpc()
    {
        GameManager.singleton.playerList[0].opponent = GameManager.singleton.playerList[1];
        GameManager.singleton.playerList[1].opponent = GameManager.singleton.playerList[0];


        
        foreach (Controller controller in GameManager.singleton.playerList)
        {
            foreach (PlayerKeep pk in FindObjectsOfType<PlayerKeep>())
            {
                if (pk.IsOwner && controller.IsOwner)
                {
                    controller.instantiatedCaste = pk;
                }
                if (!pk.IsOwner && !controller.IsOwner)
                {
                    controller.instantiatedCaste = pk;
                }
            }
            if (controller.IsHost && controller.IsOwner)
            {
                controller.LocalPlaceCastle(new Vector3Int(-7, 0, 0));
            }
            if (!controller.IsHost && controller.IsOwner)
            {
                controller.LocalPlaceCastle(new Vector3Int(8, 0, 0));
            }
            Debug.Log("controller " + GameManager.singleton.playerList.Count);

            controller.StartGameCoroutine();
        }
        
    }
    private void StartGameCoroutine()
    {
        cardsInDeck = new List<CardInHand>( demonDeck );
        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        SpawnHUDAndHideOnAllNonOwners();
        turn += OnTurn;
        resources = new PlayerResources();
        resourcesChanged += UpdateHudForResourcesChanged;


        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost && IsOwner)
            {
                col = colorsToPickFrom[0];
            }
            if (!IsHost && IsOwner)
            {
                col = colorsToPickFrom[1];
            }

            if (!IsOwner && IsHost)
            {
                col = colorsToPickFrom[1];
            }
            if (!IsOwner && !IsHost)
            {
                col = colorsToPickFrom[0];
            }
        }
        else
        {
            col = colorsToPickFrom[0];
        }
        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        GameManager.singleton.Shuffle(cardsInDeck);
        for (int i = 0; i < 7; i++) 
        {
                resources.greenManaCap++;
                resources.greenMana++;
                resources.blackManaCap++;
                resources.blackMana++;
                resources.whiteManaCap++;
                resources.whiteMana++;
                resources.blueManaCap++;
                resources.blueMana++;
                resources.redManaCap++;
                resources.redMana++;
            DrawCard();
        }
    }

    private void LocalChooseDeckForPlayer(string selectedDeck)
    {
        Deck deckChosenAsInts = JsonUtility.FromJson<Deck>(selectedDeck);
        List<CardInHand> translatedCards = new List<CardInHand>();
        for (int i = 0; i < deckChosenAsInts.deck.Count; i++)
        {
            translatedCards.Add(CardCollectionData.singleton.GetCardAssociatedWithType((SpellSiegeData.Cards)deckChosenAsInts.deck[i]));
        }
        cardsInDeck = new List<CardInHand>();
        cardsInDeck = translatedCards;
        cardsInDeck = GameManager.singleton.Shuffle(cardsInDeck);

    }

    private readonly Vector3 serverCastlePosition = new Vector3(-9, 0, 0);
    private readonly Vector3 clientCastlePosition = new Vector3(10, 0, 0);

    


    protected void GrabAllObjectsFromGameManager()
    {
        canvasMain = GameManager.singleton.canvasMain.GetComponent<Canvas>();
        highlightTile = GameManager.singleton.highlightTile;
        highlightMap = GameManager.singleton.highlightMap;// set these = to gamemanage.singleton.highlightmap TODO
        baseMap = GameManager.singleton.baseMap;
        environmentMap = GameManager.singleton.enviornmentMap;
        waterMap = GameManager.singleton.waterTileMap;
        grid = GameManager.singleton.grid;
        castle = GameManager.singleton.castleTransform;
        GameManager.singleton.playerList.Add(this);
    }
    protected void SpawnHUDAndHideOnAllNonOwners()
    {
        instantiatedPlayerUI = Instantiate(playerHud, canvasMain.transform);

        cardParent = instantiatedPlayerUI.GetComponent<HudElements>().cardParent;
        Debug.Log($"IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}");

        if (!IsOwner)
        {
            instantiatedPlayerUI.gameObject.SetActive(false);
        }
        else
        {
            cardParent.gameObject.GetComponent<Image>().color = transparentCol;
            hudElements = instantiatedPlayerUI.GetComponent<HudElements>();
            hudElements.UpdateHudVisuals(this, turnThreshold);
            instantiatedPlayerUI.gameObject.SetActive(true);
        }
    }

    protected virtual void OnTurn()
    {
        StartTurnPhase();
    }



    public virtual void StartTurnPhase()
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
        }
    }


    // Update is called once per frame
    protected virtual void Update()
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
            var screenPoint = Input.mousePosition;
            screenPoint.z = Camera.main.transform.position.y - 3; //distance of the plane from the camera
            Vector3 cardPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            locallySelectedCard.transform.position = new Vector3(cardPosition.x, cardPosition.y, cardPosition.z);
        }
        if (Input.GetMouseButtonDown(1))
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (locallySelectedCard != null)
            {
                if (cellPositionSentToClients != null)
                {
                    if (cellPositionSentToClients != grid.WorldToCell(mousePosition))
                    {
                        if (!CheckForRaycast())
                        {
                            LeftClickQueue(grid.WorldToCell(mousePosition));
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
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
                    LeftClickQueue(cellPositionSentToClients);
                }
            }
            else
            {
                LeftClickQueue(cellPositionSentToClients);
            }

        }

    }
    void LeftClickQueue(Vector3Int positionSent)
    {
        Debug.Log("Local creature " + locallySelectedCard);
        //visual section for spawning creatures
        if (locallySelectedCard != null && locallySelectedCard.cardType == SpellSiegeData.CardType.Creature)
        {
            if (CheckToSeeIfCanSpawnCreature(positionSent))
            {
                SpawnVisualCreatureOnTile(positionSent);
                LeftClickBaseMapServerRpc(positionSent);
            }
            return;
        }
        if (state == State.PlacingCastle)
        {
            LeftClickBaseMapServerRpc(positionSent);
        }
    }

    public void SetVisualsToNothingSelectedLocally()
    {

        if (locallySelectedCard != null)
        {
            if (locallySelectedCardInHandToTurnOff != null)
            {
                locallySelectedCardInHandToTurnOff.gameObject.SetActive(true);
            }
            Destroy(locallySelectedCard.gameObject);
        }

        foreach (BaseTile bt in highlightedTiles)
        {
            bt.UnHighlightTile();
        }
        highlightedTiles.Clear();


    }

    private void FixedUpdate()
    {
        if (GameManager.singleton == null)
        {
            return;
        }
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


    private void HandleTurn()
    {
        turnTimer++;
        if (hudElements != null)
        {
            hudElements.UpdateDrawSlider(turnTimer);
        }
        if (turnTimer > turnThreshold)
        {
            //turn.Invoke();
            turnTimer = 0;
        }

    }

    protected void TriggerAllCreatureAbilities()
    {
        foreach (KeyValuePair<int, Creature> kp in creaturesOwned)
        {
            kp.Value.OnTurn();
        }
        foreach (KeyValuePair<Vector3Int, BaseTile> kp in tilesOwned)
        {
            if (kp.Value.CreatureOnTile() != null && kp.Value.CreatureOnTile().playerOwningCreature == kp.Value.playerOwningTile)
            {
                kp.Value.CreatureOnTile().Garrison();
            }
        }
    }

    protected void HandleDrawCards()
    {
        DrawCard();
    }


    protected void HandleMana()
    {
        numberOfLandsYouCanPlayThisTurn = 1;
        numberOfLandsPlayedThisTurn = 0;
        ClearMana();
        AddToMana();
    }



    void AddIndexOfCardInHandToTickQueueLocal(int index)
    {
        Debug.Log("got here card");
        SelectCardInHandServerRpc(index);
    }
    void AddIndexOfCreatureOnBoard(int index)
    {
        SelectCreatureOnBoardServerRpc(index);
    }



    void LocalLeftClick(Vector3Int positionSent)
    {
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                break;
            case State.CreatureInHandSelected:
                Debug.Log("Local left click ddqwwd");
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
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in BaseMapTileState.singleton.baseTiles)
        {
            bt.Value.SetAllNeighborTiles();
        }

        placedCellPosition = positionSent;
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            return;
        }
        //Vector3 positionToSpawn = baseMap.GetCellCenterWorld(placedCellPosition);
        SetOwningTile(placedCellPosition);

        foreach (BaseTile neighbor in BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).neighborTiles)
        {
            foreach (BaseTile neighborOfNeighbor in neighbor.neighborTiles)
            {
                SetOwningTile(neighborOfNeighbor.tilePosition);
            }
        }
        //instantiatedCaste = Instantiate(castle, positionSent, Quaternion.identity);
        //instantiatedCaste.GetComponent<MeshRenderer>().material.color = col;
        //AddStructureToTile(instantiatedCaste.GetComponent<Structure>(), positionSent);
    }

    protected void AddStructureToTile(Structure structure, Vector3Int positionSent)
    {
        BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).structureOnTile = structure;
        structure.tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent);
        structure.playerOwningStructure = this;
        structuresOwned.Add(structure);
    }

    public virtual void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Green)
        {
            resources.greenManaCap++;
            resources.greenMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Black)
        {
            resources.blackManaCap++;
            resources.blackMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.White)
        {
            resources.whiteManaCap++;
            resources.whiteMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Blue)
        {
            resources.blueManaCap++;
            resources.blueMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Red)
        {
            resources.redManaCap++;
            resources.redMana++;
        }
        if (!harvestedTiles.Contains(baseTileSent))
        {
            harvestedTiles.Add(baseTileSent);
            baseTileSent.SetBeingHarvested();
        }
        baseTileSent.ShowHarvestIcon();
        //baseTileSent.HighlightTile();
        numberOfLandsPlayedThisTurn++;

        resourcesChanged.Invoke(resources);
    }




    CardInHand locallySelectedCardInHandToTurnOff;
    bool CheckForRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHitCardInHand, Mathf.Infinity))
        {
            if (raycastHitCardInHand.transform.GetComponent<CardInHand>() != null)
            {
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                if (raycastHitCardInHand.transform.GetComponent<CardInHand>().isPurchasable)
                {
                    SetVisualsToNothingSelectedLocally();
                    //todo
                    locallySelectedCardInHandToTurnOff = raycastHitCardInHand.transform.GetComponent<CardInHand>();
                    locallySelectedCardInHandToTurnOff.TurnOffVisualCard();
                    locallySelectedCard = Instantiate(locallySelectedCardInHandToTurnOff.gameObject, canvasMain.transform).GetComponent<CardInHand>();
                    locallySelectedCard.transform.position = locallySelectedCardInHandToTurnOff.transform.position;
                    locallySelectedCard.transform.localEulerAngles = Vector3.zero;
                    locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);
                    AddIndexOfCardInHandToTickQueueLocal(locallySelectedCardInHandToTurnOff.indexOfCard);
                    ShowViablePlacableTiles(locallySelectedCardInHandToTurnOff);
                    return true;
                }
            }
        }
        if (Physics.Raycast(ray, out RaycastHit raycastHitCreatureOnBoard, Mathf.Infinity, creatureMask))
        {
            if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>() != null)
            {
                if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>().playerOwningCreature == this && state != State.SpellInHandSelected)
                {
                    SetVisualsToNothingSelectedLocally();
                    AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                    return true;
                }
                if (state == State.SpellInHandSelected || state == State.StructureInHandSeleced)
                {
                    SetVisualsToNothingSelectedLocally();
                    if (cardSelected.GameObjectToInstantiate != null)
                    {
                        if (cardSelected.GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
                        {
                            AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    List<BaseTile> highlightedTiles = new List<BaseTile>();
    private void ShowViablePlacableTiles(CardInHand locallySelectedCardInHandToTurnOff)
    {
        Debug.Log("Showing viable placable tiles");
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Creature)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                kvp.Value.HighlightTile();
                highlightedTiles.Add(kvp.Value);
            }
        }
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Structure)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                if (!harvestedTiles.Contains(kvp.Value))
                {
                    kvp.Value.HighlightTile();
                    highlightedTiles.Add(kvp.Value);
                }
            }
        }
        /*
    if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Structure)
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
        {
            foreach (BaseTile neighbor in kvp.Value.neighborTiles)
            {
                if (!tilesOwned.ContainsValue(neighbor))
                {
                    neighbor.HighlightTile();
                    highlightedTiles.Add(neighbor);
                }
            }
        }*/
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Spell)
        {
            if (locallySelectedCardInHandToTurnOff.GameObjectToInstantiate.GetComponent<Spell>() != null)
            {
                if (locallySelectedCardInHandToTurnOff.GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
                {
                    foreach (BaseTile bt in harvestedTiles)
                    {
                        bt.HighlightTile();
                        highlightedTiles.Add(bt);
                    }
                }
            }
        }
    }

    private void TargetACreature(Creature creatureToTarget, Creature targetingCreature)
    {
        if (IsOwner)
        {
            targetingCreature.SetTargetToFollow(creatureToTarget, targetingCreature.actualPosition);
            TargetACreatureServerRpc(targetingCreature.creatureID, creatureToTarget.creatureID, targetingCreature.actualPosition);
            SetVisualsToNothingSelectedLocally();
        }
    }
    private void TargetACreatureLocal(int selectedCreatureID, int creatureToTargetID, Vector3 actualPosition)
    {
        if (!IsOwner)
        {
            GameManager.singleton.allCreaturesOnField[selectedCreatureID].SetTargetToFollow(GameManager.singleton.allCreaturesOnField[creatureToTargetID], actualPosition);
        }
    }


    protected void LocalSelectCardWithIndex(int indexOfCardSelected)
    {
        CardInHand cardToSelect;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].indexOfCard == indexOfCardSelected)
            {
                cardToSelect = cardsInHand[i];
                cardSelected = cardToSelect;
                if (cardSelected.cardType == SpellSiegeData.CardType.Creature)
                {
                    state = State.CreatureInHandSelected;
                }
                if (cardSelected.cardType == SpellSiegeData.CardType.Spell)
                {
                    state = State.SpellInHandSelected;
                }
                if (cardSelected.cardType == SpellSiegeData.CardType.Structure)
                {
                    state = State.StructureInHandSeleced;
                }
            }
        }

    }

    public Dictionary<int, Creature> creaturesOwned = new Dictionary<int, Creature>();


    void HandleCreatureInHandSelected(Vector3Int cellSent)
    {
        SpendManaToCast(cardSelected);
        CastCreatureOnTile(cardSelected, cellSent);
        SetStateToNothingSelected();
    }
    protected virtual bool CheckToSeeIfCanSpawnCreature(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == SpellSiegeData.traversableType.SwimmingAndFlying && locallySelectedCard.GameObjectToInstantiate.GetComponent<Creature>().thisTraversableType == SpellSiegeData.travType.Walking)
        {
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).playerOwningTile == this)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).structureOnTile == null && BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).CreatureOnTile() == null)
            {
                SetVisualsToNothingSelectedLocally();
                return true;
            }
        }
        return false;
    }

    [SerializeField] Transform visualSpawnEffect; GameObject localVisualCreture;
    GameObject instantiatedSpawnPArticle;
    private void SpawnVisualCreatureOnTile(Vector3Int positionSent)
    {
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(positionSent);

        instantiatedSpawnPArticle = Instantiate(visualSpawnEffect, new Vector3(positionToSpawn.x, positionToSpawn.y + .2f, positionToSpawn.z), Quaternion.identity).gameObject;
        Destroy(locallySelectedCard.gameObject);

        locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);
    }

    public void CastCreatureOnTile(CardInHand cardSelectedSent, Vector3Int cellSent)
    {
        Debug.Log(IsOwner + " spawning creature on tile");
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
        GameObject instantiatedCreature = Instantiate(cardSelectedSent.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
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
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>().creatureID, instantiatedCreature.GetComponent<Creature>());
        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent);
        instantiatedCreature.GetComponent<Creature>().OnETB();

        cardSelectedSent.transform.parent = null;
        RemoveCardFromHand(cardSelectedSent);
        if (instantiatedSpawnPArticle != null)
        {
            Destroy(instantiatedSpawnPArticle);
        }
        instantiatedCreature.GetComponent<Creature>().SetStructureToFollow(opponent.instantiatedCaste, instantiatedCreature.GetComponent<Creature>().actualPosition);
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
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && !cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                RemoveCardFromHand(cardSelected);
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                if (harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
                {
                    Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                    RemoveCardFromHand(cardSelected);
                    SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                    GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                    instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                    OnSpellCast();
                    SetVisualsToNothingSelectedLocally();
                    SetStateToNothingSelected();
                    return;
                }
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
            if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)) && tilesOwned.ContainsValue(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
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

                SpendManaToCast(cardSelected.GetComponent<CardInHand>());

                foreach (BaseTile bt in BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).neighborTiles)
                {
                    SetOwningTile(bt.tilePosition);
                }
                AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent));
                RemoveCardFromHand(cardSelected);
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
        }

    }



    protected void SpendManaToCast(CardInHand cardSelectedSent)
    {
        resources.blueMana -= cardSelectedSent.blueManaCost;
        resources.redMana -= cardSelectedSent.redManaCost;
        resources.whiteMana -= cardSelectedSent.whiteManaCost;
        resources.blackMana -= cardSelectedSent.blackManaCost;
        resources.greenMana -= cardSelectedSent.greenManaCost;
        SpendGenericMana(cardSelectedSent.genericManaCost);
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

    protected void SetOwningTile(Vector3Int cellPosition)
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
            //DiscardCard();
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

    private void DiscardCard()
    {
        cardsInHand[0].DiscardAnimation();
        RemoveCardFromHand(cardsInHand[0]);
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
            if (cardToRemove.cardType != SpellSiegeData.CardType.Creature)
            {
                cardToRemove.DiscardAnimation();
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
                CastSpellOnTargetedCreature(creatureSelectedSent);
            }
            return;
        }
    }

    private void CastSpellOnTargetedCreature(Creature creatureSelectedSent)
    {
        Debug.Log(cardSelected + " card selected send to casting spell on target creature");
        SpendManaToCast(cardSelected.GetComponent<CardInHand>());
        GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, creatureSelectedSent.tileCurrentlyOn.tilePosition, Quaternion.identity);
        instantiatedSpell.GetComponent<TargetedSpell>().InjectDependencies(creatureSelectedSent, this);
        OnSpellCast();
        RemoveCardFromHand(cardSelected);
        SetStateToNothingSelected();
    }

    public void SetStateToNothingSelected()
    {
        if (locallySelectedCard != null)
        {
            locallySelectedCard = null;
            //SetVisualsToNothingSelectedLocally();
        }
        cardSelected = null;

        state = State.NothingSelected;
    }
    protected void SetStateToWaiting()
    {
        cardSelected = null;
        state = State.Waiting;
    }

    public virtual void ClearMana()
    {
        resources.blueMana = 0;
        resources.greenMana = 0;
        resources.redMana = 0;
        resources.blackMana = 0;
        resources.whiteMana = 0;

        resourcesChanged.Invoke(resources);
    }
    public virtual void AddToMana()
    {
        for (int i = 0; i < harvestedTiles.Count; i++)
        {
            for (int j = 0; j < harvestedTiles[i].currentAmountOfManaProducing; j++)
            {
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Blue)
                {
                    resources.blueMana++;
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Black)
                {
                    resources.blackMana++;
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Red)
                {
                    resources.redMana++;
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.White)
                {
                    resources.whiteMana++;
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Green)
                {
                    resources.greenMana++;
                }
            }
        }

        resourcesChanged.Invoke(resources);
    }
    public void AddSpecificManaToPool(SpellSiegeData.ManaType manaTypeSent)
    {
        if (manaTypeSent == SpellSiegeData.ManaType.Black)
        {
            resources.blackMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.Blue)
        {
            resources.blueMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.Red)
        {
            resources.redMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.Green)
        {
            resources.greenMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.White)
        {
            resources.whiteMana++;
        }

        resourcesChanged.Invoke(resources);
    }
    int totalMana;
    public virtual void UpdateHudForResourcesChanged(PlayerResources resources)
    {
        if (IsOwner)
        {
            hudElements.UpdateHudElements(resources);
        }
        CheckAffordableCards();
        totalMana = resources.blackMana + resources.blueMana + resources.whiteMana + resources.greenMana + resources.redMana;
    }


    public void CheckAffordableCards()
    {

        foreach (CardInHand cardInHand in cardsInHand)
        {
            cardInHand.CheckToSeeIfPurchasable(resources);
        }
    }




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
            int randomNumber = UnityEngine.Random.Range(0, cardsInHand.Count - 1);
            if (numbersChosen.Contains(randomNumber))
            {
                break;
            }
            numbersChosen.Add(randomNumber);
            if (cardsInHand[randomNumber].cardType == SpellSiegeData.CardType.Creature)
            {
                creatureSelectedInHand = cardsInHand[randomNumber];
            }
        }
        return creatureSelectedInHand;
    }






    [ServerRpc]
    public void ChooseDeckServerRpc(string selectedDeck)
    {
        ChooseDeckClientRpc(selectedDeck);
    }
    [ClientRpc]
    public void ChooseDeckClientRpc(string selectedDeck)
    {
        LocalChooseDeckForPlayer(selectedDeck);
    }


    [ServerRpc]
    internal void AttackCreatureServerRpc(int creatureAttacking, int creatureBeingAttacked)
    {
        AttackCreatureClientRpc(creatureAttacking, creatureBeingAttacked);
    }
    [ClientRpc]
    internal void AttackCreatureClientRpc(int creatureAttacking, int creatureBeingAttacked)
    {
        GameManager.singleton.allCreaturesOnField[creatureAttacking].LocalAttackCreature(GameManager.singleton.allCreaturesOnField[creatureBeingAttacked]);
    }
    [ServerRpc]
    internal void AttackStructureServerRpc(int creatureAttacking, Vector3Int positionOfStructure)
    {
        AttackStructureClientRpc(creatureAttacking, positionOfStructure);
    }
    [ClientRpc]
    internal void AttackStructureClientRpc(int creatureAttacking, Vector3Int positionOfStructure)
    {
        GameManager.singleton.allCreaturesOnField[creatureAttacking].LocalAttackStructure(BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionOfStructure).structureOnTile);
    }

    [ServerRpc]
    public void GiveCounterServerRpc(int creatureID, int numOfCounters)
    {
        GiveCounterClientRpc(creatureID, numOfCounters);
    }
    [ClientRpc]
    private void GiveCounterClientRpc(int creatureID, int numOfCounters)
    {
        GameManager.singleton.allCreaturesOnField[creatureID].LocalGiveCounter(numOfCounters);
    }
    [ServerRpc]
    internal void KillCreatureWithoutRequiringOwnershipServerRpc(int creatureID)
    {
        KillCreatureWithoutRequiringOwnershipClientRpc(creatureID);
    }
    [ClientRpc]
    internal void KillCreatureWithoutRequiringOwnershipClientRpc(int creatureID)
    {
        GameManager.singleton.allCreaturesOnField[creatureID].LocalDie();
    }
    [ServerRpc]
    internal void SetCreatureToIdleServerRpc(int creatureID, Vector3Int actualPosition)
    {
        SetCreatureToIdleClientRpc(creatureID, actualPosition);
    }
    [ClientRpc]
    internal void SetCreatureToIdleClientRpc(int creatureID, Vector3Int actualPosition)
    {
        GameManager.singleton.allCreaturesOnField[creatureID].LocalSetCreatureToIdle(actualPosition);
    }
    [ServerRpc]
    internal void DieServerRpc(int creatureToDie)
    {
        DieClientRpc(creatureToDie);
    }
    [ClientRpc]
    internal void DieClientRpc(int creatureToDie)
    {
        GameManager.singleton.allCreaturesOnField[creatureToDie].LocalDie();
    }
    internal void LocalDie(int creatureToDie)
    {
        Debug.Log("killing creature " + creatureToDie);
        GameManager.singleton.allCreaturesOnField[creatureToDie].LocalDie();
    }

    [ServerRpc]
    private void TargetACreatureServerRpc(int selectedCreatureID, int creatureToTargetID, Vector3 actualPosition)
    {
        TargetACreatureClientRpc(selectedCreatureID, creatureToTargetID, actualPosition);
    }
    [ClientRpc]
    private void TargetACreatureClientRpc(int selectedCreatureID, int creatureToTargetID, Vector3 actualPosition)
    {
        TargetACreatureLocal(selectedCreatureID, creatureToTargetID, actualPosition);
    }
    [ServerRpc]
    private void SelectCardInHandServerRpc(int cardIndex)
    {
        SelectCardInHandClientRpc(cardIndex);
    }
    [ClientRpc]
    private void SelectCardInHandClientRpc(int cardIndex)
    {
        LocalSelectCardWithIndex(cardIndex);
    }
    [ServerRpc]
    private void LeftClickBaseMapServerRpc(Vector3Int positionSent)
    {
        LeftClickBaseMapClientRpc(positionSent);
    }
    [ClientRpc]
    private void LeftClickBaseMapClientRpc(Vector3Int positionSent)
    {
        LocalLeftClick(positionSent);
    }
    [ServerRpc]
    private void SelectCreatureOnBoardServerRpc(int creatureIDSent)
    {
        SelectCreatureOnBoardClientRpc(creatureIDSent);
    }
    [ClientRpc]
    private void SelectCreatureOnBoardClientRpc(int creatureIDSent)
    {
        SetToCreatureOnFieldSelected(GameManager.singleton.allCreaturesOnField[creatureIDSent]);
    }

}



public struct PlayerResources
{
    public int blueMana;
    public int redMana;
    public int whiteMana;
    public int blackMana;
    public int greenMana;


    public int blueManaCap;
    public int redManaCap;
    public int whiteManaCap;
    public int blackManaCap;
    public int greenManaCap;

}