using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leylineoftheharvest : Spell
{
    //TODO MAKE IT SO YOU CAN ONLLY CAST ON YOUR TILEs
    public override void Update()
    {
    }


    protected override void SpecificCastEffect()
    {
        base.SpecificCastEffect();
        BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition).IncreaseAmountOfManaProducing();
    }
}
