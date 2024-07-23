using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMana : Spell
{
    protected override void SpecificCastEffect()
    {
        base.SpecificCastEffect();
        for (int i = 0; i < 2; i++)
        {
            playerCastingSpell.AddSpecificManaToPool(SpellSiegeData.ManaType.Red);
        }
    }

}
