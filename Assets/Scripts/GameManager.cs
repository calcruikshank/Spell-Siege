using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject aiPrefab;
    public static GameManager singleton;
    public State state;
    public Grid grid;
    public Tilemap baseMap;
    public Tilemap enviornmentMap;
    public Tilemap waterTileMap;
    public Tilemap highlightMap;
    public Material RenderInFrontMat;
    public Material TransparentSharedMat;
    public Material rangeIndicatorMat;
    public Material OpaqueSharedMat;
    public Transform castleTransform;
    public TileBase highlightTile;
    public GameObject highlightForBaseTiles;

    public Transform cardParent;
    public Transform canvasMain;
    public List<Controller> playerList = new List<Controller>();
    public List<Controller> playersThatHavePlacedCastle = new List<Controller>();
    public List<Controller> playersThatHaveBeenReceived = new List<Controller>();

    public Transform canAttackIcon;
    [SerializeField] public Camera mainCam;
    public Dictionary<int, Creature> allCreaturesOnField = new Dictionary<int, Creature>();

    //public float tickTimeAverage;
    int playerCount; //TODO set this equal to players in scene and return if a player has not hit

    public int allCreatureGuidCounter;

    public int endingX;
    public int endingY;
    public int startingX = -10;
    public int startingY = 9;

    public Transform blueManaSymbol;
    public Transform redManaSymbol;
    public Transform greenManaSymbol;
    public Transform blackManaSymbol;
    public Transform whiteManaSymbol;

    public Transform purchasableGlow;

    [SerializeField] TextMeshPro damageText;

    [SerializeField] public Transform onDeathEffect;
    Transform instantiatedDamageText;
    Transform instantiatedHealthText;

    [SerializeField] public Canvas RectCanvas;
    [SerializeField] public Canvas scalableUICanvas;

    public bool hasStartedGame = false;
    public int turnTimer;


    public VisualAttackParticle rangedVisualAttackParticle;
    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
    }

    void Start()
    {
        Debug.Log(IsHost + " is host");

        if (!IsHost)
        {
            ClientLoadedGameServerRpc();
        }
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            OnServerStopped();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        foreach (Controller controller in playerList)
        {
            if (controller.OwnerClientId == clientId)
            {
                // Call the ClientRpc to notify other clients
                NotifyClientsOfDisconnectionClientRpc(clientId);
                break;
            }
        }
    }

    private void OnServerStopped()
    {// Despawn or Destroy network objects
        DespawnAllNetworkObjects();
        // Notify all clients that the server has stopped
        NotifyClientsOfServerEndClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientLoadedGameServerRpc()
    {
        foreach (ulong clientID in NetworkManager.ConnectedClientsIds)
        {
            SpawnPlayerPrefabs(clientID);
        }
    }

    private void SpawnPlayerPrefabs(ulong clientID)
    {
        if (playerPrefab != null)
        {
            // Instantiate the player prefab
            GameObject playerInstance = Instantiate(playerPrefab);

            // Get the NetworkObject component
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                // Spawn the object on the network with the given client ID
                networkObject.SpawnAsPlayerObject(clientID);
            }
            else
            {
                Debug.LogError("Player prefab does not have a NetworkObject component.");
            }
        }
        else
        {
            Debug.LogError("Player prefab is not assigned.");
        }
    }

    private void FixedUpdate()
    {
        if (hasStartedGame)
        {
            turnTimer += 1;
        }
    }

    public enum State
    {
        Setup, //The state for placing your castle
        Game,
        End //Setup for scaling
    }

    public List<CardInHand> Shuffle(List<CardInHand> alpha)
    {
        for (int i = 0; i < alpha.Count; i++)
        {
            CardInHand temp = alpha[i];
            int randomIndex = UnityEngine.Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
        return alpha;
    }

    internal void AddPlayerToReady(Controller controllerSent)
    {
        //state = State.Game;
        playersThatHavePlacedCastle.Add(controllerSent);

        if (playersThatHavePlacedCastle.Count >= playerList.Count)
        {
            hasStartedGame = true;
            foreach (Controller controller in playerList)
            {
                Shuffle(controller.cardsInDeck);
                for (int i = 0; i < 6; i++)
                {
                    controller.DrawCard();
                }
                controller.StartTurnPhase();
            }
        }
    }

    public void SpawnDamageText(Vector3 positionSent, float damageSent)
    {
        Transform instantiatedDamageText = Instantiate(damageText.transform, positionSent, Quaternion.identity);
        instantiatedDamageText.localEulerAngles = new Vector3(90, 0, 0);
        instantiatedDamageText.GetComponent<TextMeshPro>().text = damageSent.ToString();
    }

    [SerializeField] Transform healParticle;
    internal void SpawnHealText(Vector3 positionSent, float amount)
    {
        Transform instantiatedHealthText = Instantiate(damageText.transform, positionSent, Quaternion.identity);
        instantiatedHealthText.localEulerAngles = new Vector3(90, 0, 0);
        instantiatedHealthText.GetComponent<TextMeshPro>().text = amount.ToString();
        instantiatedHealthText.GetComponent<TextMeshPro>().color = Color.green;
        Instantiate(healParticle, positionSent, Quaternion.identity);
    }

    internal void SpawnLevelUpPrefab(Vector3 positionSent)
    {
    }

    internal void CreatureDied(int creatureID)
    {
        if (allCreaturesOnField[creatureID] != null)
        {
            foreach (KeyValuePair<int, Creature> kvp in allCreaturesOnField)
            {
                kvp.Value.OtherCreatureDied(allCreaturesOnField[creatureID]);
            }
        }
    }

    internal void CreatureEntered(int creatureID)
    {
        foreach (KeyValuePair<int, Creature> kvp in allCreaturesOnField)
        {
            kvp.Value.OtherCreatureEntered(allCreaturesOnField[creatureID]);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc(ulong playerOwningStructureID)
    {
        foreach (Controller controller in playerList)
        {
            if (controller.OwnerClientId != playerOwningStructureID)
            {
                // Call the ClientRpc to execute WinGame on the owning client
                WinGameClientRpc(controller.OwnerClientId);
            }
        }

        // Despawn or Destroy network objects
        DespawnAllNetworkObjects();

        // Load the game scene on the server and clients
        ReloadGameScene();
    }


    private async void ReloadGameScene()
    {
        // Lock the lobby if necessary (assuming you have a matchmaking service)
        await MatchmakingService.LockLobby();

        // Load the scene on the server
        NetworkManager.Singleton.SceneManager.LoadScene("SimpleGame", LoadSceneMode.Single);

        // Call the client RPC to load the scene on all clients
        LoadGameSceneClientRpc();
    }

    [ClientRpc]
    private void LoadGameSceneClientRpc()
    {
        if (!IsServer)
        {
            SceneManager.LoadScene("SimpleGame");
        }
    }

    [ClientRpc]
    private void WinGameClientRpc(ulong clientId)
    {
        // Find the controller for the given clientId and call WinGame
        foreach (Controller controller in playerList)
        {
            if (controller.OwnerClientId == clientId && controller.IsOwner)
            {
                controller.WinGame();
                break;
            }
        }
    }

    [ClientRpc]
    private void NotifyClientsOfDisconnectionClientRpc(ulong disconnectedClientId)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null.");
            return;
        }

        if (IsHost)
        {
            // Despawn all network objects
            DespawnAllNetworkObjects();

            // Stop the host
            NetworkManager.Singleton.Shutdown();
        }

        if (NetworkManager.Singleton.LocalClientId != disconnectedClientId)
        {
            // Call the WinGame method on clients that did not disconnect
            foreach (Controller controller in playerList)
            {
                if (controller.OwnerClientId != disconnectedClientId && controller.IsOwner)
                {
                    controller.WinGame();
                }
            }

            // Load the main menu scene
            SceneHandler.Instance.LoadMainMenu();
        }
    }

    private void DespawnAllNetworkObjects()
    {
        foreach (var networkObject in FindObjectsOfType<NetworkObject>())
        {
            if (networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
        }
    }


    [ClientRpc]
    private void NotifyClientsOfServerEndClientRpc()
    {
        // Call the WinGame method on all remaining clients
        foreach (Controller controller in playerList)
        {
            if (controller.IsOwner)
            {
                controller.WinGame();
            }
        }

        // Load the main menu scene
        SceneHandler.Instance.LoadMainMenu();
    }
}
