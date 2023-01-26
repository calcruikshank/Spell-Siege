using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonWhisperer : Creature
{
    public override void OnETB()
    {
        base.OnETB();
        foreach (CardInHand cardindeck in playerOwningCreature.cardsInHand)
        {
            if (cardindeck.creatureType == SpellSiegeData.CreatureType.Dragon)
            {
                if (cardindeck.redManaCost > 0)
                {
                    cardindeck.redManaCost--;
                    cardindeck.UpdateMana();
                }
            }
        }
    }
}
