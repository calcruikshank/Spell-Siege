using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : TargetedSpell
{

    protected override void SpecificSpellAbility()
    {
        creatureTargeted.TakeDamage(7);
    }
    
}
