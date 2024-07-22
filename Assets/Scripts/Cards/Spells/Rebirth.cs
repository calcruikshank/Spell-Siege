using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rebirth : TargetedSpell
{

    protected override void SpecificSpellAbility()
    {
        Debug.Log(creatureTargeted.numberOfTimesThisCanDie + " number of times this cvan die");
        creatureTargeted.numberOfTimesThisCanDie++;
        Debug.Log(creatureTargeted.numberOfTimesThisCanDie + " number of times this cvan die");
    }
}
