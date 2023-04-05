using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardsCollectedForPlayer
{
    public List<int> cardsCollected = new List<int>();

    public int numberOfCorePacks = 10;
}
