using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldspanDragon : FlyingCreature
{
    public override void OnAttack()
    {
        playerOwningCreature.AddSpecificManaToPool(SpellSiegeData.ManaType.Red);
    }
}
