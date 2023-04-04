using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThenasDeterminedEnvionmentalist : Creature
{
    public override void OtherCreatureEntered(Creature creatureThatEntered)
    {
        //TODO check if garrisoned (within player keep radius)
        if (playerOwningCreature.tilesOwned.ContainsValue(this.tileCurrentlyOn))
        {
            //Destroy that creature and play a harvest card on place of death
            if (creatureThatEntered != this)
            {
                playerOwningCreature.AddTileToHarvestedTilesList(creatureThatEntered.tileCurrentlyOn);
                if (playerOwningCreature.IsOwner)
                {
                    creatureThatEntered.Die();
                }
            }
        }
    }
}
