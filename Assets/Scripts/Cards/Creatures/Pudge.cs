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
        swallowedCreature.SetStateToExiled();
        tempLineRendererBetweenCreaturesGameObject.SetActive(false);

    }

    public override void Garrison()
    {
        base.Garrison();
        if (playerOwningCreature.IsOwner)
        {
            if (swallowedCreature != null)
            {
                GiveCounter((int)swallowedCreature.CurrentHealth);
                swallowedCreature.actualPosition = this.actualPosition;
                swallowedCreature.transform.position = this.actualPosition;
                playerOwningCreature.KillCreatureWithoutRequiringOwnershipServerRpc(swallowedCreature.creatureID);
                swallowedCreature = null;

            }
            
        }
    }

    public override void OnDeath()
    {
        if (swallowedCreature != null)
        {
            swallowedCreature.gameObject.SetActive(true);
            swallowedCreature.creatureState = CreatureState.Idle;
            swallowedCreature.targetedPosition = this.actualPosition;
            swallowedCreature.SetStateToIdle();
        }
        swallowedCreature = null;
        base.OnDeath();
    }
}
