using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avacyn : FlyingCreature
{
    List<Creature> creaturesDoubled = new List<Creature>();
    protected override void CheckForCreaturesWithinRange()
    {
        base.CheckForCreaturesWithinRange();

        MakeIndestructible();

    }

    private void MakeIndestructible()
    {
        if (creaturesDoubled != null)
        {
            foreach (Creature friendly in creaturesDoubled)
            {
                if (!friendlyCreaturesWithinRange.Contains(friendly))
                {
                    friendly.ToggleIndestructibilty(false);
                }
            }
        }
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesDoubled.Contains(friendly))
            {
                friendly.ToggleIndestructibilty(true);
            }
        }
        creaturesDoubled = friendlyCreaturesWithinRange;
    }

    public override void OnDeath()
    {
        foreach (Creature friendly in creaturesDoubled)
        {
            friendly.ToggleIndestructibilty(false);
        }
        base.OnDeath();
        
    }
}
