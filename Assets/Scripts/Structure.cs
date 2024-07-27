using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : NetworkBehaviour
{
    [SerializeField] TextMeshPro keepHealth;
    Grid grid;
    public Controller playerOwningStructure;
    Tilemap baseTileMap;
    public Vector3Int currentCellPosition;

    [SerializeField] float health = 30;
    public BaseTile tileCurrentlyOn;

    private void Start()
    {
        grid = GameManager.singleton.grid;
        baseTileMap = GameManager.singleton.baseMap;
        currentCellPosition = grid.WorldToCell(this.transform.position);
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);

        
        Debug.Log(tileCurrentlyOn);
        tileCurrentlyOn.AddStructureToTile(this); 
        
        
        if (keepHealth != null)
        {
            keepHealth.text = health.ToString();
        }
    }

    private void OnDestroy()
    {
        tileCurrentlyOn.RemoveStructureFromTile(this);
    }

    internal void TakeDamage(float amountofdamage)
    {
        health -= amountofdamage;
        if (health <= 0)
        {
            if (IsOwner)
            {
                GameManager.singleton.EndGameServerRpc(this.OwnerClientId);
            }
        }
        if (keepHealth != null)
        {
            keepHealth.text = health.ToString();
        }
    }

    private void DestroyStructure()
    {
        Destroy(this.gameObject);
    }

    internal void InjectDependencies(Vector3Int cellSent, Controller controller)
    {
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent);
        playerOwningStructure = controller;
    }
}
