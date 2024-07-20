using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wildebeest : Creature
{
    public List<Creature> creaturesYouveDealtDamageTo = new List<Creature>();
    public override void Move()
    {
        currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
        if (BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition) == null)
        {
            tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
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
            actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z), speed * Time.fixedDeltaTime * 2);

        }

        Creature creatureOnCurrentTile = BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition);
        if (creatureOnCurrentTile != null)
        {
            if (!creaturesYouveDealtDamageTo.Contains(creatureOnCurrentTile))
            {
                creaturesYouveDealtDamageTo.Add(creatureOnCurrentTile);
                VisualAttackAnimation(creatureOnCurrentTile);
            }
        }
    }

    public override void ChooseTarget()
    {

    }
}
