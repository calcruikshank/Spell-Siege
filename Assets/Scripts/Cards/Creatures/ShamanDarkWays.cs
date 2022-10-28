using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamanDarkWays : Creature
{
    bool didHeal = false;
    protected override void HandleFriendlyCreaturesList()
    {
        if (abilityRateTimer > abilityRate)
        {
            foreach (Creature friendlyCreature in friendlyCreaturesWithinRange)
            {
                if (friendlyCreature.CurrentHealth < friendlyCreature.MaxHealth)
                {
                    friendlyCreature.Heal(Attack);
                    didHeal = true;
                }
            }
            if (didHeal) 
            {
                didHeal = false;
                abilityRateTimer = 0;
            }
        }
    }
}
