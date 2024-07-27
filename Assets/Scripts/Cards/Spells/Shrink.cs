using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrink : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.attack = 1;
        creatureTargeted.currentAttack = 1;
        creatureTargeted.MaxHealth = 1;
        creatureTargeted.CurrentHealth = 1;
    }
}
