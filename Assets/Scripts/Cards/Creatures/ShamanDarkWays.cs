using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamanDarkWays : Creature
{
    bool didHeal = false;
    protected override void HandleFriendlyCreaturesList()
    {
            foreach (Creature friendlyCreature in friendlyCreaturesWithinRange)
            {
                if (friendlyCreature.CurrentHealth < friendlyCreature.MaxHealth)
                {
                    friendlyCreature.Heal(currentAttack);
                    didHeal = true;
                }
            }
            if (didHeal) 
            {
                didHeal = false;
            }
    }
}
