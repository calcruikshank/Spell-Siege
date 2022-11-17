using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritSummoner : Creature
{
    public override void OnDeath()
    {
        base.OnDeath();
        CardInHand randomCreatureInHand = playerOwningCreature.GetRandomCreatureInHand();
        if (randomCreatureInHand != null)
        {
            playerOwningCreature.CastCreatureOnTile(randomCreatureInHand, this.tileCurrentlyOn.tilePosition);
        }
    }
}
