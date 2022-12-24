using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public delegate void TickTookTooLong(int numberOfTicksPast);
    public static event TickTookTooLong tickTookTooLong;
    public bool hasTicked = false;

    public bool anyPlayerMadeInput = false;

    public static TickManager singleton;
    // Start is called before the first frame update

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
    }
    void Start()
    {
        GameManager.singleton.tick += OnTick;
    }

    private void OnTick()
    {
        hasTicked = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTicked == true && anyPlayerMadeInput)
        {
            Debug.Log(GameManager.singleton.numOfFixedUpdatesItTookToReceiveAllPlayers + " tick timer on received players");
            hasTicked = false;
            anyPlayerMadeInput = false;
            tickTookTooLong?.Invoke(GameManager.singleton.numOfFixedUpdatesItTookToReceiveAllPlayers);
        }
    }
}
