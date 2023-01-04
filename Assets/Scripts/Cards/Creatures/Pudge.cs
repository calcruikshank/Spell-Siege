using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pudge : Creature
{
    Creature swallowedCreature;
    public override void LocalAttackCreature(Creature creatureToEat)
    {
        if (swallowedCreature == null)
        {
            Swallow(creatureToEat);
            OnAttack();

        }
    }


    private void Swallow(Creature creatureToEat)
    {
        swallowedCreature = creatureToEat;
        creatureToEat.gameObject.SetActive(false);
    }

    public override void Garrison()
    {
        base.Garrison();
        if (playerOwningCreature.IsOwner)
        {
            if (swallowedCreature != null)
            {
                GiveCounter((int)swallowedCreature.CurrentHealth);
                playerOwningCreature.KillCreatureWithoutRequiringOwnershipServerRpc(swallowedCreature.creatureID);
            }
            
        }
    }

    public override void OnDeath()
    {
        if (swallowedCreature != null)
        {
            swallowedCreature.gameObject.SetActive(true);
            swallowedCreature.actualPosition = this.actualPosition;
        }
        swallowedCreature = null;
        base.OnDeath();
    }
}
