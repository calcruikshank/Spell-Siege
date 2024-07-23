using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.HandleAttack();
        playerCastingSpell.DrawCard();
    }
}
