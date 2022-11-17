using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorStrike : Spell
{
    protected override void SpecificCastEffect()
    {
        foreach (BaseTile bt in allTilesWithinRange)
        {
            if (bt.CreatureOnTile() != null)
            {
                bt.CreatureOnTile().TakeDamage(damage);
            }
        }
    }
}
