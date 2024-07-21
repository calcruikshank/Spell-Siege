using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvercommitedCleric : Creature
{
    public Creature creatureToHEal = new Creature();
    protected override void HandleFriendlyCreaturesList()
    {
        float lowestHealthCreatureValue = -1;
        creatureToHEal = new Creature();
        base.HandleFriendlyCreaturesList();
        for (int i = 0; i < friendlyCreaturesWithinRange.Count; i++)
        {
            if (lowestHealthCreatureValue == -1 && friendlyCreaturesWithinRange[i].CurrentHealth < friendlyCreaturesWithinRange[i].MaxHealth)
            {
                lowestHealthCreatureValue = friendlyCreaturesWithinRange[i].CurrentHealth; creatureToHEal = friendlyCreaturesWithinRange[i];
            }
            if (friendlyCreaturesWithinRange[i])
            {
                if (friendlyCreaturesWithinRange[i].CurrentHealth < friendlyCreaturesWithinRange[i].MaxHealth)
                {
                    if (lowestHealthCreatureValue > friendlyCreaturesWithinRange[i].CurrentHealth)
                    {
                        lowestHealthCreatureValue = friendlyCreaturesWithinRange[i].CurrentHealth;
                        creatureToHEal = friendlyCreaturesWithinRange[i];
                    }
                }
            }
        }
        if (creatureToHEal != null)
        {
            creatureToHEal.Heal(this.currentAttack);
        }
    }
}
