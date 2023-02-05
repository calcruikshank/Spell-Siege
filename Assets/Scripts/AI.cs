using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : Controller
{
    protected override void Start()
    {
        isAI = true;
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

    float checkAITimer = 1f;
    float decisionTimer = 0;
    protected override void Update()
    {
        decisionTimer += Time.deltaTime;
        if (decisionTimer >= checkAITimer)
        {
            decisionTimer = 0;
            switch (state)
            {
                case State.PlacingCastle:
                    PlaceCastle();
                    break;
                case State.NothingSelected:
                    CheckForAnyCreaturesOnField();
                    CheckForAnyCreaturesYouCanAfford();
                    CheckForAnyStructuresOnField();
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

    }

    private void CheckForAnyStructuresOnField()
    {
        foreach (Controller c in GameManager.singleton.playersThatHavePlacedCastle)
        {
            if (c != this)
            {
                foreach (KeyValuePair<int, Creature> creatureOwned in creaturesOwned)
                {
                    if (creatureOwned.Value.targetToFollow == null && creatureOwned.Value.structureToFollow == null)
                    {
                        for (int i = 0; i < c.structuresOwned.Count; i++)
                        {
                            creatureOwned.Value.SetStructureToFollow(c.structuresOwned[i], creatureOwned.Value.actualPosition);
                        }
                    }
                    else
                    {
                    }
                }
            }
        }
    }

    private void CheckForAnyCreaturesOnField()
    {
        foreach (KeyValuePair<int, Creature> kvp in creaturesOwned)
        {
            if (kvp.Value.creatureState != Creature.CreatureState.Moving)
            {
                foreach (Controller c in GameManager.singleton.playersThatHavePlacedCastle)
                {
                    if (c != this)
                    {
                        foreach (KeyValuePair<int, Creature> co in c.creaturesOwned)
                        {
                            if (co.Value != null)
                            {
                                kvp.Value.SetTargetToFollow(co.Value, kvp.Value.actualPosition);
                            }
                        }
                    }
                }
            }
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
        SetStateToNothingSelected();
    }
    protected override bool CheckToSeeIfCanSpawnCreature(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == SpellSiegeData.traversableType.SwimmingAndFlying && cardSelected.GameObjectToInstantiate.GetComponent<Creature>().thisTraversableType == SpellSiegeData.travType.Walking)
        {
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).playerOwningTile == this)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).structureOnTile == null && BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).CreatureOnTile() == null)
            {
                SetVisualsToNothingSelectedLocally();
                return true;
            }
        }
        return false;
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
        placedCellPosition = FindForestMountainTiles();
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

    private Vector3Int FindForestMountainTiles()
    {
        for (int x = GameManager.singleton.startingX; x < GameManager.singleton.endingX; x++)
        {
            for (int y = GameManager.singleton.startingY; y < GameManager.singleton.endingY; y++)
            {
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(x, y)).manaType == SpellSiegeData.ManaType.Green)
                {
                    return new Vector3Int(x, y);
                }
            }
        }
        return Vector3Int.zero;
        Debug.Log("couldnt fid green");
    }

    public override void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        Debug.Log(resources.greenMana + " green mana available");
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
        resourcesChanged.Invoke(resources);

    }
    public override void ClearMana()
    {
        resources.blueMana = 0;
        resources.greenMana = 0;
        resources.redMana = 0;
        resources.blackMana = 0;
        resources.whiteMana = 0;
        resourcesChanged.Invoke(resources);

    }
}
