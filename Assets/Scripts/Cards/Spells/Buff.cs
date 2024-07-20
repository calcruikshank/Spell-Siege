using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.GiveCounter(3);
    }
}
