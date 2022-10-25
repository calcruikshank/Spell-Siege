using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : MonoBehaviour
{

    Grid grid;
    public Controller playerOwningStructure;
    Tilemap baseTileMap;
    public Vector3Int currentCellPosition;

    public float health = 100;
    public BaseTile tileCurrentlyOn;

    private void Start()
    {
        grid = GameManager.singleton.grid;
        baseTileMap = GameManager.singleton.baseMap;
        currentCellPosition = grid.WorldToCell(this.transform.position);
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        tileCurrentlyOn.AddStructureToTile(this);
    }

    private void OnDestroy()
    {
        tileCurrentlyOn.RemoveStructureFromTile(this);
    }

    internal void TakeDamage(float amountofdamage)
    {
        health -= amountofdamage;
    }
}
