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
        creatureToEat.enabled = false;
        swallowedCreature.creatureState = CreatureState.Dead;
        
    }

    public override void HandleSpecialUpdates()
    {
        base.HandleSpecialUpdates();
        if (swallowedCreature != null)
        {
            swallowedCreature.actualPosition = this.actualPosition;
        }
    }

    public override void Garrison()
    {
        base.Garrison();
        if (swallowedCreature != null)
        {
            for (int i = 0; i < swallowedCreature.CurrentHealth; i++)
            {
                GiveCounter(1);
            }
            swallowedCreature.Kill();
            swallowedCreature = null;
        }
    }

    public override void OnDeath()
    {
        if (swallowedCreature != null)
        {
            swallowedCreature.enabled = true;
            swallowedCreature.gameObject.SetActive(true);
            swallowedCreature.actualPosition = this.actualPosition;
        }
        swallowedCreature = null;
        base.OnDeath();
    }
}
