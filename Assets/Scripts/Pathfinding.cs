using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding
{
   Grid grid;
   Tilemap baseMap;
    private List<BaseTile> openList;
    private List<BaseTile> closedList;
    Creature.travType travTypeSent;
    public List<BaseTile> FindPath(Vector3Int startingPosition, Vector3Int endingPosition, Creature.travType travTypeOfCreature)
    {
        travTypeSent = travTypeOfCreature;
        openList = new List<BaseTile>();
        BaseTile startingTile = BaseMapTileState.singleton.GetBaseTileAtCellPosition(startingPosition);
        BaseTile endingTile = BaseMapTileState.singleton.GetBaseTileAtCellPosition(endingPosition);
        openList = new List<BaseTile> { startingTile };
        closedList = new List<BaseTile>();

        for (int x = GameManager.singleton.startingX; x < GameManager.singleton.endingX; x++)
        {
            for (int y = GameManager.singleton.startingY; y < GameManager.singleton.endingY; y++) 
            {
                BaseTile baseTile = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(x, y, 0));
                //baseTile.gameObject.SetActive(false);
                baseTile.gCost = int.MaxValue;
                baseTile.CalculateFCost();
                baseTile.cameFromBaseTile = null;
            }

        }

        startingTile.gCost = 0;
        startingTile.hCost = CalculateDistanceCost(startingTile, endingTile);
        startingTile.CalculateFCost();

        while (openList.Count > 0)
        {
            BaseTile currentTile = GetTheLowestFCostNode(openList);

            if (currentTile == endingTile)
            {
                return CalculatePath(endingTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (BaseTile neighbor in currentTile.neighborTiles)
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }
                if (neighbor.CreatureOnTile() != null)
                {
                    continue;
                }
                switch (travTypeSent)
                {
                    case Creature.travType.Flying:
                        if (neighbor.traverseType == BaseTile.traversableType.Untraversable)
                        {
                            continue;
                        }
                        break;
                    case Creature.travType.Walking:
                        if (neighbor.traverseType == BaseTile.traversableType.Untraversable)
                        {
                            continue;
                        }
                        if (neighbor.traverseType == BaseTile.traversableType.OnlyFlying)
                        {
                            continue;
                        }
                        if (neighbor.traverseType == BaseTile.traversableType.SwimmingAndFlying)
                        {
                            continue;
                        }
                        break;
                }
                int tentativeGCost = currentTile.gCost + CalculateDistanceCost(currentTile, neighbor);
                if (tentativeGCost < neighbor.gCost)
                {
                    neighbor.cameFromBaseTile = currentTile;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = CalculateDistanceCost(neighbor, endingTile);
                    neighbor.CalculateFCost();

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

        }


        return null;
    }

    List<BaseTile> CalculatePath(BaseTile endingTileSent)
    {
        List<BaseTile> path = new List<BaseTile>();
        path.Add(endingTileSent);
        BaseTile currentNode = endingTileSent;

        while (currentNode.cameFromBaseTile != null)
        {
            path.Add(currentNode.cameFromBaseTile);
            currentNode = currentNode.cameFromBaseTile;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(BaseTile a, BaseTile b)
    {
        int potentialRange = Mathf.Abs(a.tilePosition.x - b.tilePosition.x);
        if (Mathf.Abs(a.tilePosition.y - b.tilePosition.y) > potentialRange)
        {
            potentialRange = Mathf.Abs(a.tilePosition.y - b.tilePosition.y);
        }

        int xDistance = Mathf.Abs(a.tilePosition.x - b.tilePosition.x);
        int xThreshold = potentialRange - xDistance;
        int threshold = potentialRange + xThreshold;
        int yDistance = Mathf.Abs(a.tilePosition.y - b.tilePosition.y);
        int remaining = 0;

        /*if (a.tilePosition.y % 2 == 0)
        {
            if (b.tilePosition.x < a.tilePosition.x )
            {
                threshold++;
            }
        } 
        if (a.tilePosition.y % 2 != 0)
        {
            if (b.tilePosition.x > a.tilePosition.x )
            {
                threshold++;
            }
        }*/
        if (threshold < xDistance + yDistance)
        {
            remaining = (Math.Abs(threshold - (xDistance + yDistance)));
        }
        return (potentialRange + remaining) * 10;
    }

    private BaseTile GetTheLowestFCostNode(List<BaseTile> baseTileList)
    {
        BaseTile lowestFCostBaseTile = baseTileList[0];
        for (int i = 0; i < baseTileList.Count; i++)
        {
            if (baseTileList[i].fCost < lowestFCostBaseTile.fCost)
            {
                lowestFCostBaseTile = baseTileList[i];
            }
        }

        return lowestFCostBaseTile;
    }
}
