using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField]public int range = 2;
    protected List<BaseTile> allTilesWithinRange = new List<BaseTile>();
    protected List<Vector3> rangePositions = new List<Vector3>();
    protected Vector3Int currentCellPosition;
    protected Controller playerCastingSpell;

    [SerializeField] Transform particleToInstantiate;
    Transform instantiatedParticle;
    [SerializeField] protected int damage = 3;
    float lifetime;

    public bool SpellRequiresToBeCastOnAHarvestedTile = false;
    public void InjectDependencies(Vector3Int targetTile, Controller playerCasting)
    {
        currentCellPosition = targetTile;
        playerCastingSpell = playerCasting;
        SetRangeLineRenderer();
        if (range != 0)
        {
            CalculateAllTilesWithinRange();
        }
        Cast();
    }
    void CalculateAllTilesWithinRange()
    {
        List<Vector3Int> extents = new List<Vector3Int>();
        allTilesWithinRange.Clear();
        rangePositions.Clear();

        int xthreshold;
        int threshold;
        for (int x = 0; x < range + 1; x++)
        {
            for (int y = 0; y < range + 1; y++)
            {
                xthreshold = range - x;
                threshold = range + xthreshold;

                if (y + x > threshold)
                {

                    if (currentCellPosition.y % 2 == 0)
                    {
                        if (y + x <= threshold + 1)
                        {
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z)));
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z)));
                        }
                    }
                    if (currentCellPosition.y % 2 != 0)
                    {
                        if (y + x <= threshold + 1)
                        {
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z)));
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z)));
                        }
                    }
                    continue;
                }
                if (y == range && currentCellPosition.y % 2 == 0)
                {
                    if (range % 2 != 0 && y + x == threshold - 1)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                    if (range % 2 == 0 && y + x == threshold)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                }
                if (y == range && currentCellPosition.y % 2 != 0)
                {
                    if (range % 2 != 0 && y + x == threshold - 1)
                    {
                        extents.Add(new Vector3Int(x + 1, y, currentCellPosition.z));
                    }
                    if (range % 2 == 0 && y + x == threshold)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                }
                if (x == range && y + x == threshold)
                {
                    extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z)));
                }

            }
        }

        extents.Add(new Vector3Int(extents[0].x, -extents[0].y, extents[0].z));
        if (range % 2 != 0)
        {
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, -extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x - 1, -extents[0].y, extents[0].z));
            }
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, +extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x - 1, +extents[0].y, extents[0].z));
            }
        }
        if (range % 2 == 0)
        {
            extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            extents.Add(new Vector3Int(-extents[0].x, +extents[0].y, extents[0].z));
            /*if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            }
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, +extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, +extents[0].y, extents[0].z));
            }*/
        }
        if (range != 0)
        {
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).topRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[1]).topRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[1]).bottomRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[2]).bottomRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[2]).bottom);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[3]).bottom);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[3]).bottomLeft);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[4]).bottomLeft);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[4]).topLeft);

            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[5]).topLeft);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[5]).top);

            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);
        }
        if (range == 0)
        {

            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[1]).topRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[2]).bottomRight);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[3]).bottom);
            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[4]).bottomLeft);

            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[5]).topLeft);

            rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);
        }


            List<Vector3> newRangePositions = new List<Vector3>();


        SetNewPositionsForRangeLr(rangePositions);
    }
    void SetNewPositionsForRangeLr(List<Vector3> rangePositionsSent)
    {
        //rangeLr.enabled = true;

        rangeLr.enabled = true;
        rangeLr.positionCount = rangePositionsSent.Count;
        rangeLr.SetPositions(rangePositionsSent.ToArray());
    }
    LineRenderer rangeLr;
    GameObject rangeLrGO;
    private void SetRangeLineRenderer()
    {
        rangeLrGO = new GameObject("LineRendererGameObjectForRange", typeof(LineRenderer));
        rangeLr = rangeLrGO.GetComponent<LineRenderer>();
        rangeLr.enabled = false;
        rangeLr.alignment = LineAlignment.TransformZ;
        rangeLr.transform.localEulerAngles = new Vector3(90, 0, 0);
        rangeLr.sortingOrder = 1000;
        rangeLr.startWidth = .2f;
        rangeLr.endWidth = .2f;
        rangeLr.numCapVertices = 1;
        rangeLr.material = GameManager.singleton.RenderInFrontMat;
        rangeLr.startColor = playerCastingSpell.col;
        rangeLr.endColor = playerCastingSpell.col;
    }

    protected virtual void Cast()
    {
        if (particleToInstantiate != null)
        {
            instantiatedParticle = Instantiate(particleToInstantiate, BaseMapTileState.singleton.GetWorldPositionOfCell(currentCellPosition), Quaternion.identity);
        }
        SpecificCastEffect();
        

    }

    protected virtual void SpecificCastEffect()
    {
    }

    public virtual void Update()
    {
        if (instantiatedParticle != null)
        {
            lifetime += Time.deltaTime;
            if (lifetime >= instantiatedParticle.GetComponent<ParticleSystem>().main.duration)
            {
                Destroy(rangeLrGO);
                Destroy(instantiatedParticle.gameObject);
            }
        }
    }
}
