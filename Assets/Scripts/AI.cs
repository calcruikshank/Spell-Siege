using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : Controller
{
    protected override void Start()
    {
        GrabAllObjectsFromGameManager();
        turn += OnTurn;
        SpawnHUDAndHideOnAllNonOwners();
        instantiatedPlayerUI.gameObject.SetActive(false);
        resources = new PlayerResources();
        resourcesChanged += UpdateHudForResourcesChanged;
        mousePositionScript = GetComponent<MousePositionScript>();

        state = State.SelectingDeck;
        col = colorsToPickFrom[1];
        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;

        List<CardInHand> translatedCards = new List<CardInHand>();
        translatedCards = dragonDeck;
        cardsInDeck = new List<CardInHand>();
        cardsInDeck = translatedCards;
        cardsInDeck = GameManager.singleton.Shuffle(cardsInDeck);

        SetStateToPlacingCastle();
    }

    protected override void Update()
    {
        switch (state)
        {
            case State.PlacingCastle:
                PlaceCastle();
                break;
            case State.NothingSelected:
                CheckForAnyCreaturesYouCanAfford();
                break;
            case State.CreatureInHandSelected:
                PlayCreature();
                break;
            case State.SpellInHandSelected:
                break;
            case State.StructureInHandSeleced:
                break;
        }
    }

    private void PlayCreature()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (CheckToSeeIfCanSpawnCreature(bt.Key))
            {
                SpendManaToCast(cardSelected);
                CastCreatureOnTile(cardSelected, bt.Key);
                SetStateToNothingSelected();
                break;
            }
        }
    }
    public override void StartTurnPhase()
    {
        HandleHarvestTiles();
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureInHandSelected:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.SpellInHandSelected:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.StructureInHandSeleced:
                HandleMana();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
        }
    }
    protected override void HandleHarvestTiles()
    {
        canPurchaseHarvestTile = true;
        if (canPurchaseHarvestTile)
        {
            BuyRandomHarvestTile();
        }
    }

    void BuyRandomHarvestTile()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (!harvestedTiles.Contains(bt.Value))
            {
                PurchaseHarvestTile(bt.Key);
                break;
            }
        }
    }
    private void CheckForAnyCreaturesYouCanAfford()
    {
        foreach (CardInHand cih in cardsInHand)
        {
            if (cih.isPurchasable)
            {
                LocalSelectCardWithIndex(cih.indexOfCard);
                break;
            }
        }
    }

    private void PlaceCastle()
    {
        placedCellPosition = new Vector3Int(0,0,0);
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition).traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            return;
        }
        Vector3 positionToSpawn = highlightMap.GetCellCenterWorld(placedCellPosition);

        SetOwningTile(placedCellPosition);
        for (int i = 0; i < BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition).neighborTiles.Count; i++)
        {
            SetOwningTile(BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition).neighborTiles[i].tilePosition);
        }
        instantiatedCaste = Instantiate(castle, positionToSpawn, Quaternion.identity);
        //instantiatedCaste.GetComponent<MeshRenderer>().material.color = col;
        AddStructureToTile(instantiatedCaste.GetComponent<Structure>(), placedCellPosition);
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(placedCellPosition));
        GameManager.singleton.AddPlayerToReady(this);
        SetStateToWaiting();
    }
    public override void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Green)
        {
            resources.greenManaCap++;
            resources.greenMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Black)
        {
            resources.blackManaCap++;
            resources.blackMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.White)
        {
            resources.whiteManaCap++;
            resources.whiteMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Blue)
        {
            resources.blueManaCap++;
            resources.blueMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Red)
        {
            resources.redManaCap++;
            resources.redMana++;
        }

        if (!harvestedTiles.Contains(baseTileSent))
        {
            harvestedTiles.Add(baseTileSent);
            baseTileSent.SetBeingHarvested();
        }
    }
    public override void ClearMana()
    {
        resources.blueMana = 0;
        resources.greenMana = 0;
        resources.redMana = 0;
        resources.blackMana = 0;
        resources.whiteMana = 0;
    }
    protected override void UpdateHudForResourcesChanged(PlayerResources resources)
    {
    }
}
