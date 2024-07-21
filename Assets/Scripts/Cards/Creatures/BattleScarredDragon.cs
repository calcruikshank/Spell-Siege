using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleScarredDragon : Creature
{
    List<Creature> creaturesDoubled = new List<Creature>();
    protected override void CheckForCreaturesWithinRange()
    {
        base.CheckForCreaturesWithinRange();

        DoubleAttack();
        
    }

    private void DoubleAttack()
    {
        if (creaturesDoubled != null)
        {
            foreach (Creature friendly in creaturesDoubled)
            {
                if (!friendlyCreaturesWithinRange.Contains(friendly))
                {
                    friendly.IncreaseAttackByX(-friendly.attack);
                }
            }
        }
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesDoubled.Contains(friendly))
            {
                friendly.IncreaseAttackByX(friendly.attack);
            }
        }
        creaturesDoubled = friendlyCreaturesWithinRange;
    }

    public override void OnDeath()
    {
        foreach (Creature friendly in creaturesDoubled)
        {
            friendly.IncreaseAttackByX(-friendly.attack);
        }
        base.OnDeath();

    }
}
