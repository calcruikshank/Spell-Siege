using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseMapTileState : MonoBehaviour
{
    public static BaseMapTileState singleton;

    Tilemap baseMap;
    Tilemap highlightMap;
    Grid grid;

    public Dictionary<Vector3Int, BaseTile> baseTiles = new Dictionary<Vector3Int, BaseTile>();
    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        baseMap = this.GetComponent<Tilemap>();
    }
    private void Start()
    {
        grid = GameManager.singleton.grid;
        highlightMap = GameManager.singleton.highlightMap;
    }
    public Controller GetOwnerOfTile(Vector3Int cellPosition)
    {
        BaseTile baseTile = GetBaseTileAtCellPosition(cellPosition);
        return baseTile.playerOwningTile;
    }

    internal Creature GetCreatureAtTile(Vector3Int cellPosition)
    {
        BaseTile baseTile = GetBaseTileAtCellPosition(cellPosition);
        return baseTile.CreatureOnTile();
    }

    internal void AddToBaseTiles(Vector3Int currentCellPosition, BaseTile baseTile)
    {
        if (baseMap.GetInstantiatedObject(currentCellPosition))
        {
            if (!baseTiles.ContainsKey(currentCellPosition))
            {
                baseTiles.Add(currentCellPosition, baseMap.GetInstantiatedObject(currentCellPosition).GetComponent<BaseTile>());
            }
            //has a non water tile
        }
        else
        {
            //is a water tile
            baseTiles.Add(currentCellPosition, baseTile);
        }
    }

    public BaseTile GetBaseTileAtCellPosition(Vector3Int currentCellPosition)
    {
        if (baseTiles.ContainsKey(currentCellPosition))
        {
            BaseTile baseTile = baseTiles[currentCellPosition];
            return baseTile;
        }
        else
        {
            return null;
        }
    }

    public BaseTile GetNearestBaseTileGivenCell(BaseTile tileComingFrom, BaseTile tileMovingTowards)
    {
        Vector3 direction = GetDirectionOfTwoTiles(tileComingFrom, tileMovingTowards);
        return tileMovingTowards.neighborTiles[0];
    }

    public Vector3 GetDirectionOfTwoTiles(BaseTile tileComingFrom, BaseTile tileMovingTowards)
    {
        Vector3 direction = new Vector3();
        direction = (grid.CellToWorld(tileComingFrom.tilePosition) - grid.CellToWorld(tileMovingTowards.tilePosition));
        Debug.Log(direction + " Direction");
        return direction;

    }

    public Vector3 GetWorldPositionOfCell(Vector3Int cellPositionSent)
    {
        Vector3 worldPositionOfCell = highlightMap.GetCellCenterWorld(cellPositionSent);
        worldPositionOfCell = new Vector3(worldPositionOfCell.x, worldPositionOfCell.y + .2f, worldPositionOfCell.z);
        if (baseMap.GetInstantiatedObject(cellPositionSent) == null)
        {
        }
        return worldPositionOfCell;
    }

    public int GetNumberOfTilesBetweenTwoTiles(BaseTile tile0, BaseTile tile1)
    {
        Vector3Int distanceVector = tile0.tilePosition - tile1.tilePosition;
        int greatestABSValueInVector = Math.Abs(distanceVector.x);
        int absOfY = Math.Abs( distanceVector.y );
        int absOfZ = Math.Abs(distanceVector.z);
        if (absOfY > greatestABSValueInVector)
        {
            greatestABSValueInVector = absOfY;
        }
        if (absOfZ > greatestABSValueInVector)
        {
            greatestABSValueInVector = absOfZ;
        }
        return greatestABSValueInVector;
    }
}
