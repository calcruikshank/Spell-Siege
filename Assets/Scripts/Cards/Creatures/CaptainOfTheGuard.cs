using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainOfTheGuard : Creature
{
    List<Creature> creaturesDoubled = new List<Creature>();
    protected override void CheckForCreaturesWithinRange()
    {
        GiveCounterToCreeaturesInRange();
        base.CheckForCreaturesWithinRange();


    }

    private void GiveCounterToCreeaturesInRange()
    {
        if (creaturesDoubled != null)
        {
            foreach (Creature friendly in creaturesDoubled)
            {
                if (!friendlyCreaturesWithinRange.Contains(friendly))
                {
                    friendly.GiveCounter(-1);
                }
            }
        }
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesDoubled.Contains(friendly))
            {
                friendly.GiveCounter(1);
            }
        }
        creaturesDoubled = friendlyCreaturesWithinRange;
    }

    public override void OnDeath()
    {
        foreach (Creature friendly in creaturesDoubled)
        {
            friendly.GiveCounter(-1);
        }
        base.OnDeath();

    }
}
