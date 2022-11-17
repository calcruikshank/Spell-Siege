using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        base.SpecificSpellAbility();
        creatureTargeted.Kill();
    }
}
