using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baboon : Creature
{
    public override void OnAttack()
    {
        base.OnAttack();
        playerOwningCreature.AddSpecificManaToPool(SpellSiegeData.ManaType.Red);
    }
}
