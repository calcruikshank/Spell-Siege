using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    public Vector3Int tilePosition;
    Grid grid;
    Tilemap environmentMap;
    Tilemap baseTileMap;
    Creature creatureOnTile;
    GameObject environmentOnTile;
    public Structure structureOnTile;

    public Controller playerOwningTile;

    public List<BaseTile> neighborTiles;

    public Tilemap currentMapThatTileIsIn;

    public int gCost;
    public int hCost;
    public int fCost;
    public BaseTile cameFromBaseTile;

    public int harvestCost;

    int harvestCostMultiplier = 3;
    public SpellSiegeData.ManaType manaType;
    [SerializeField] public SpellSiegeData.traversableType traverseType;

    private void Start()
    {
        SetupLR();
        grid = GameManager.singleton.grid;
        environmentMap = GameManager.singleton.enviornmentMap;
        baseTileMap = GameManager.singleton.baseMap;

        tilePosition = baseTileMap.WorldToCell(this.transform.position);
        BaseMapTileState.singleton.AddToBaseTiles(tilePosition, this);

        environmentOnTile = environmentMap.GetInstantiatedObject(tilePosition);

        CalculateAllPoints();

    }

    public void SetAllNeighborTiles()
    {
        if (Mathf.Abs(tilePosition.y % 2) == 1)
        {
            SetNeighborTile(new Vector3Int(tilePosition.x + 1, tilePosition.y + 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x + 1, tilePosition.y, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x + 1, tilePosition.y - 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x, tilePosition.y - 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x - 1, tilePosition.y, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x, tilePosition.y + 1, tilePosition.z));
        }
        if (Mathf.Abs(tilePosition.y % 2) == 0)
        {
            SetNeighborTile(new Vector3Int(tilePosition.x, tilePosition.y + 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x + 1, tilePosition.y, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x - 1, tilePosition.y - 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x, tilePosition.y - 1, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x - 1, tilePosition.y, tilePosition.z));
            SetNeighborTile(new Vector3Int(tilePosition.x - 1, tilePosition.y + 1, tilePosition.z));
        }
    }

    public void SetNeighborTile(Vector3Int cellPosiitonSent)
    {
        neighborTiles.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosiitonSent));
    }

    internal Creature CreatureOnTile()
    {
        return creatureOnTile;
    }
    internal Structure StructureOnTile()
    {
        return structureOnTile;
    }

    internal void AddCreatureToTile(Creature creature)
    {
        creatureOnTile = creature;
        if (environmentOnTile != null)
        {
            if (environmentOnTile.GetComponent<ChangeTransparency>() == null)
            {
                environmentOnTile.AddComponent<ChangeTransparency>().ChangeTransparent(100);
            }
            else
            {
                environmentOnTile.GetComponent<ChangeTransparency>().ChangeTransparent(100);
            }
        }
       
    }
    internal void RemoveCreatureFromTile(Creature creature)
    {
        creatureOnTile = null;
        if (environmentOnTile != null)
        {
            if (environmentOnTile.GetComponent<ChangeTransparency>() == null)
            {
                environmentOnTile.AddComponent<ChangeTransparency>().SetOpaque();
            }
            else
            {
                environmentOnTile.GetComponent<ChangeTransparency>().SetOpaque();
            }
        }
    }
    internal void AddStructureToTile(Structure structure)
    {
        structureOnTile = structure;
        if (environmentOnTile != null)
        {
            Destroy(environmentOnTile);
        }

    }
    internal void RemoveStructureFromTile(Structure structure)
    {
        structureOnTile = null;
    }

    public List<Vector3> worldPositionsOfVectorsOnGrid = new List<Vector3>();

    public Vector3 topRight;
    public Vector3 bottomRight;
    public Vector3 bottom;
    public Vector3 bottomLeft;
    public Vector3 topLeft;
    public Vector3 top;

    void CalculateAllPoints()
    {
        float y = 0;
        Vector3 worldPositionOfCell = new Vector3(this.transform.position.x, .11f, this.transform.position.z);
        topRight = new Vector3(grid.GetBoundsLocal(tilePosition).extents.x, y, grid.GetBoundsLocal(tilePosition).extents.z / 2) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(topRight);

        bottomRight = new Vector3(grid.GetBoundsLocal(tilePosition).extents.x, y, -grid.GetBoundsLocal(tilePosition).extents.z / 2) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(bottomRight);

        bottom = new Vector3(0, y, -grid.GetBoundsLocal(tilePosition).extents.z) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(bottom);
        bottomLeft = new Vector3(-grid.GetBoundsLocal(tilePosition).extents.x, y, -grid.GetBoundsLocal(tilePosition).extents.z / 2) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(bottomLeft);
        topLeft = new Vector3(-grid.GetBoundsLocal(tilePosition).extents.x, y, grid.GetBoundsLocal(tilePosition).extents.z / 2) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(topLeft);
        top = new Vector3(0, y, grid.GetBoundsLocal(tilePosition).extents.z) + worldPositionOfCell;
        worldPositionsOfVectorsOnGrid.Add(top);
        worldPositionsOfVectorsOnGrid.Add(topRight);
    }
    TextMeshPro costText;
    public void SetOwnedByPlayer(Controller playerOwningTileSent)
    {
        playerOwningTile = playerOwningTileSent;
        lr.startColor = (playerOwningTile.col);
        lr.endColor = (playerOwningTile.col);
        for (int i  = 0; i < worldPositionsOfVectorsOnGrid.Count; i++)
        {
            if (playerOwningTileSent.allVertextPointsInTilesOwned.Contains(worldPositionsOfVectorsOnGrid[i]))
            {
                playerOwningTileSent.allVertextPointsInTilesOwned.Remove(worldPositionsOfVectorsOnGrid[i]);
            }
            else
            {
                playerOwningTileSent.allVertextPointsInTilesOwned.Add(worldPositionsOfVectorsOnGrid[i]);
            }
        }
        lr.enabled = true;
        lr.positionCount = worldPositionsOfVectorsOnGrid.Count;
        lr.SetPositions(worldPositionsOfVectorsOnGrid.ToArray());
        InstantiateCorrectColorManaSymbol();

        //Instantiate(new GameObject(), new Vector3( this.transform.position.x, 1, this.transform.position.z), this.transform, );
    }

    private void MakeTextObjectForTileCost(Transform parentToSet)
    {
        GameObject instantiatedText = Instantiate(new GameObject("text", typeof(TextMeshPro)), parentToSet);
        instantiatedText.transform.localPosition = new Vector3(0, .01f, -.2f);
        costText = instantiatedText.GetComponent<TextMeshPro>();
        costText.fontSize = 20;
        costText.color = Color.white;
        costText.alignment = TextAlignmentOptions.Center;
        costText.alignment = TextAlignmentOptions.Midline;
    }

    LineRenderer lr;
    GameObject lrGameObject;
    void SetupLR()
    {
        lrGameObject = new GameObject("LineRendererGameObject", typeof(LineRenderer));
        lrGameObject.transform.parent = this.transform;
        lr = lrGameObject.GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.alignment = LineAlignment.TransformZ;
        lr.transform.localEulerAngles = new Vector3(90, 0, 0);
        lr.sortingOrder = 1;
        lr.startWidth = .1f;
        lr.endWidth = .1f;
        lr.numCapVertices = 90;
        lr.numCornerVertices = 90;
        lr.material = GameManager.singleton.RenderInFrontMat;
    }
    internal void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public bool isBeingHarvested;
    Color transparentColor;
    Color opaqueColor;
    Transform instantiatedManaSymbol;

    public int currentAmountOfManaProducing = 1;
    public void SetBeingHarvested()
    {
        if (instantiatedManaSymbol != null)
        {
            instantiatedManaSymbol.GetChild(0).gameObject.SetActive(true);
            instantiatedManaSymbol.GetComponent<Image>().color = opaqueColor;
            //costText.gameObject.SetActive(true);
        }
        isBeingHarvested = true;
    }
    public void SetNotBeingHarvested()
    {
        isBeingHarvested = false;
        instantiatedManaSymbol.GetComponent<Image>().color = transparentColor;
    }
    private void InstantiateCorrectColorManaSymbol()
    {
        if (manaType == SpellSiegeData.ManaType.Blue)
        {
            instantiatedManaSymbol = Instantiate(GameManager.singleton.blueManaSymbol, this.transform);
        }
        if (manaType == SpellSiegeData.ManaType.Black)
        {
            instantiatedManaSymbol = Instantiate(GameManager.singleton.blackManaSymbol, this.transform);
        }
        if (manaType == SpellSiegeData.ManaType.White)
        {
            instantiatedManaSymbol = Instantiate(GameManager.singleton.whiteManaSymbol, this.transform);
        }
        if (manaType == SpellSiegeData.ManaType.Red)
        {
            instantiatedManaSymbol = Instantiate(GameManager.singleton.redManaSymbol, this.transform);
        }
        if (manaType == SpellSiegeData.ManaType.Green)
        {
            instantiatedManaSymbol = Instantiate(GameManager.singleton.greenManaSymbol, this.transform);
        }
        transparentColor = instantiatedManaSymbol.GetComponent<Image>().color;
        opaqueColor = instantiatedManaSymbol.GetComponent<Image>().color;
        opaqueColor.a = playerOwningTile.col.a;
        transparentColor.a = playerOwningTile.transparentCol.a;
        instantiatedManaSymbol.GetComponent<Image>().color = opaqueColor;
        instantiatedManaSymbol.transform.parent = GameManager.singleton.scalableUICanvas.transform;
        HideHarvestIcon();
        //MakeTextObjectForTileCost(instantiatedManaSymbol);
    }

    internal void HideHarvestIcon()
    {
        if (!isBeingHarvested)
        {
            instantiatedManaSymbol.gameObject.SetActive(false);
        }
    }

    internal void ShowHarvestIcon()
    {
        instantiatedManaSymbol.gameObject.SetActive(true);
    }
    float lifeTime;

    Color highlightTileCOlor = new Color();
    private void Update()
    {
        if (instantiatedManaSymbol != null)
        {
            instantiatedManaSymbol.transform.position = GameManager.singleton.mainCam.WorldToScreenPoint(this.transform.position);
            
            instantiatedManaSymbol.transform.localScale = Vector3.one * 10 * highlightMultiplierForManaSymbol / instantiatedManaSymbol.transform.position.z;
        }
        if (instantiatedHighlightTile != null)
        {
            highlightTileCOlor = Color.white;
            //get the objects current position and put it in a variable so we can access it later with less code
            //calculate what the new Y position will be
            float newY = Mathf.Sin(Time.time * 4) * .2f;
            //set the object's Y to the new calculated Y

            highlightTileCOlor.a = newY + .8f;
            instantiatedHighlightTile.GetComponent<MeshRenderer>().material.color = highlightTileCOlor;
        }
    }

    public void IncreaseAmountOfManaProducing()
    {
        currentAmountOfManaProducing++;

        instantiatedManaSymbol.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "+" + currentAmountOfManaProducing;

        if (playerOwningTile != null)
        {
            playerOwningTile.AddTileToHarvestedTilesList(this);
        }
    }

    GameObject instantiatedHighlightTile;
    internal void HighlightTile()
    {
        if (instantiatedHighlightTile == null)
        {

            instantiatedHighlightTile = Instantiate(GameManager.singleton.highlightForBaseTiles, this.transform);
            instantiatedHighlightTile.transform.position = new Vector3(this.transform.position.x, .03f, this.transform.position.z);
        }
        else
        {
            instantiatedHighlightTile.gameObject.SetActive(true);
        }
    }
    internal void UnHighlightTile()
    {
        if (instantiatedHighlightTile != null)
        {
            instantiatedHighlightTile.gameObject.SetActive(false);
        }
    }

    
    float highlightMultiplierForManaSymbol = 1;
    /*public void OnMouseEnterTile()
    {
        if (playerOwningTile != null)
        {
                if (playerOwningTile.tilesOwned.ContainsValue(this))
                {
                    if (!playerOwningTile.harvestedTiles.Contains(this))
                    {
                        highlightMultiplierForManaSymbol = 2f;
                    }
                }
        }
    }
    public void OnMouseExitTile()
    {
        if (highlightMultiplierForManaSymbol != 1)
        {
            highlightMultiplierForManaSymbol = 1f;
        }
    }*/
}
