using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panther : Creature
{
    public List<Creature> creaturesYouveDealtDamageTo = new List<Creature>();
    public override void Move()
    {
        Creature creatureOnCurrentTile = BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition);
        if (creatureOnCurrentTile != null && creatureOnCurrentTile != this && creatureOnCurrentTile.playerOwningCreature != this)
        {
            if (!creaturesYouveDealtDamageTo.Contains(creatureOnCurrentTile))
            {
                creaturesYouveDealtDamageTo.Add(creatureOnCurrentTile);
                VisualAttackAnimation(creatureOnCurrentTile);
            }
        }
        currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
        if (BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition) == null)
        {
            tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        }
        if (previousTilePosition != tileCurrentlyOn)
        {
            CalculateAllTilesWithinRange();
            previousTilePosition.RemoveCreatureFromTile(this);
            previousTilePosition = tileCurrentlyOn;
            tileCurrentlyOn.AddCreatureToTile(this);
        }
        if (previousTilePosition != tileCurrentlyOn)
        {
            //previousTilePosition.RemoveCreatureFromTile(this);
            previousTilePosition = tileCurrentlyOn;
            //tileCurrentlyOn.AddCreatureToTile(this);
        }
        if (currentTargetedStructure != null)
        {
            targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            if (currentTargetedStructure.currentCellPosition.x < this.currentCellPosition.x)
            {
                targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - 1, currentCellPosition.y, currentCellPosition.z));
                targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - 1, currentCellPosition.y, currentCellPosition.z));

                animatorForObject.transform.localEulerAngles = new Vector3(0, -90, 0);
            }
            else
            {
                targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + 1, currentCellPosition.y, currentCellPosition.z));
                targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + 1, currentCellPosition.y, currentCellPosition.z));

                animatorForObject.transform.localEulerAngles = new Vector3(0, 90, 0);
            }
            animatorForObject.SetTrigger("Run");
            actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z), speed * Time.fixedDeltaTime);

        }

        if (targetedCell.structureOnTile != null || targetedCell.traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            targetedCell = tileCurrentlyOn;

            bool creatureCanMove = false;
            if (Vector3.Distance(actualPosition, new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z)) < .05f)
            {
                SetStateToIdle();
            }
        }

    }

    public override void ChooseTarget()
    {

    }
}
