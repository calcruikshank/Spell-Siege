using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
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

    public Transform cardParent;
    public Transform canvasMain;
    public List<Controller> playerList = new List<Controller>();
    public List<Controller> playersThatHavePlacedCastle = new List<Controller>();
    public List<Controller> playersThatHaveBeenReceived = new List<Controller>();

    public Dictionary<int, Creature> allCreaturesOnField = new Dictionary<int, Creature>();
    public delegate void Tick();
    public event Tick tick;

    public int gameManagerTick = 0;
    //public float tickTimeAverage;
    int playerCount; //TODO set this equal to players in scene and return if a player has not hit


    public int tickTimer;
    public int tickTimerThreshold = 10;
    public int allCreatureGuidCounter;

    public int endingX;
    public int endingY;
    public int startingX;
    public int startingY;

    public Transform blueManaSymbol;
    public Transform redManaSymbol;
    public Transform greenManaSymbol;
    public Transform blackManaSymbol;
    public Transform whiteManaSymbol;


    [SerializeField] TextMeshPro damageText;
    Transform instantiatedDamageText;
    Transform instantiatedHealthText;
    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
        canvasMain = FindObjectOfType<Canvas>().transform;
    }
    private void Update()
    {
    }

    void Start()
    {
        if (!IsHost)
        {
            Debug.Log("Spawn players");
            SpawnPlayersServerRpc();
        }
        if (IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
            {
                SpawnPlayersServerRpc();
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    void SpawnPlayersServerRpc()
    {
        SpawnPlayersClientRpc();
    }
    [ClientRpc]
    private void SpawnPlayersClientRpc()
    {
        if (IsHost)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
            {
                GameObject instantiatedObject = Instantiate(playerPrefab);
                instantiatedObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        tickTimer++;
        if (tickTimer >= tickTimerThreshold && playersThatHaveBeenReceived.Count == playerList.Count)
        {
            tickTimer = 0;
            playersThatHaveBeenReceived.Clear();
            tick.Invoke();
            gameManagerTick++;
            //tickTimeAverage = totalTickTime / gameManagerTick;
            //allPlayersReceived = true;
        }
    }
    public enum State
    {
        Setup, //The state for placing your castle
        Game,
        End //Setup for scaling
    }
    public void AddToPlayersThatHaveBeenReceived(Controller controller)
    {
        playersThatHaveBeenReceived.Add(controller);
        if (playersThatHaveBeenReceived.Count == playerList.Count)
        {
        }
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

    internal void AddPlayerToReady(Controller controller)
    {
        playersThatHavePlacedCastle.Add(controller);

        if (playersThatHavePlacedCastle.Count == playerList.Count)
        {
            state = State.Game;
            foreach (Controller player in playerList)
            {
                player.StartGame();
                gameManagerTick = 0;
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

    internal void CreatureDied(Creature creatureSent)
    {
        foreach (KeyValuePair<int, Creature> kvp in allCreaturesOnField)
        {
            kvp.Value.OtherCreatureDied(creatureSent);
        }
    }

}
