using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritSummoner : Creature
{
    public override void OnDeath()
    {
        CardInHand randomCreatureInHand = playerOwningCreature.GetRandomCreatureInHand();
        if (randomCreatureInHand != null)
        {
            playerOwningCreature.CastCreatureOnTile(randomCreatureInHand, this.tileCurrentlyOn.tilePosition);
        }
        base.OnDeath();
        
    }
}
