using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElvishMystic : Creature
{
    public override void Garrison()
    {
        if (playerOwningCreature.tilesOwned.ContainsValue( tileCurrentlyOn ))
        {
            playerOwningCreature.AddSpecificManaToPool(tileCurrentlyOn.manaType);
        }
    }
}
