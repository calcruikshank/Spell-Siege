using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public delegate void TickTookTooLong();
    public static event TickTookTooLong tickTookTooLong;
    public bool hasPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.singleton.tick += OnTick;
    }

    private void OnTick()
    {
        hasPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.singleton.tickTimer > GameManager.singleton.tickTimerThreshold && hasPaused == false)
        {
            hasPaused = true;

            tickTookTooLong?.Invoke();
        }
    }
}
