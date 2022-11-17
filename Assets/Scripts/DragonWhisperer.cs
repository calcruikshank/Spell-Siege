using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonWhisperer : Creature
{
    public override void OnETB()
    {
        foreach (CardInHand cardindeck in playerOwningCreature.cardsInHand)
        {
            if (cardindeck.GameObjectToInstantiate.GetComponent<Creature>() != null)
            {
                if (cardindeck.GameObjectToInstantiate.GetComponent<Creature>().creatureType == CreatureType.Dragon)
                {
                    if (cardindeck.redManaCost > 0)
                    {
                        cardindeck.redManaCost--;
                    }
                    cardindeck.UpdateMana();
                }
            }
        }
    }
}
