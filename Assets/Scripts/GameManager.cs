using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour
{
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
    public int creatureGuidCounter;
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

    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
    }
    private void Update()
    {
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
}
