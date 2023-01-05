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

    public Transform canAttackIcon;
    [SerializeField] public Camera mainCam;
    public Dictionary<int, Creature> allCreaturesOnField = new Dictionary<int, Creature>();

    //public float tickTimeAverage;
    int playerCount; //TODO set this equal to players in scene and return if a player has not hit

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

    public Transform purchasableGlow;


    [SerializeField] TextMeshPro damageText;

    [SerializeField] public Transform onDeathEffect;
    Transform instantiatedDamageText;
    Transform instantiatedHealthText;

    [SerializeField] public Canvas RectCanvas;
    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
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

    [ServerRpc(RequireOwnership = false)]
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
}
