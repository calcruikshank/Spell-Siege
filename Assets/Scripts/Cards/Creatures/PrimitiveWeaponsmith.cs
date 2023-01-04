using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveWeaponsmith : Creature
{
    List<Creature> creaturesThatHaveExtraRange = new List<Creature>();
    protected override void CheckForCreaturesWithinRange()
    {
        base.CheckForCreaturesWithinRange();

        GivePlusOneRange();

    }

    private void GivePlusOneRange()
    {
        if (creaturesThatHaveExtraRange != null)
        {
            foreach (Creature friendly in creaturesThatHaveExtraRange)
            {
                if (!friendlyCreaturesWithinRange.Contains(friendly))
                {
                    friendly.SubtractOneRange();
                }
            }
        }
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesThatHaveExtraRange.Contains(friendly))
            {
                if (friendly.range == 1)
                {
                    friendly.visualAttackParticle = this.visualAttackParticle;
                }

                friendly.AddOneRange();
            }
        }
        creaturesThatHaveExtraRange = friendlyCreaturesWithinRange;
    }

    public override void OnDeath()
    {
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            friendly.SubtractOneRange();
        }
        base.OnDeath();
    }
}
