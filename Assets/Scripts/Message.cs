using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Message 
{
    public List<Vector3Int> localTilePositionsToBePurchased = new List<Vector3Int>();
    public List<Vector3Int> leftClicksWorldPos = new List<Vector3Int>();
    public List<int> guidsForCards = new List<int>();
    public List<int> guidsForCreatures = new List<int>();
    //public float timeBetweenLastTick = 0f;
}
