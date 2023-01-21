using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellPortal : Spell
{
    int indexOfCardSelected = -1;
    protected override void SpecificCastEffect()
    {
        int iterator = 0;
        while (indexOfCardSelected == -1)
        {
            if (iterator >= playerCastingSpell.cardsInDeck.Count) break;
            if (playerCastingSpell.cardsInDeck[iterator].cardType == SpellSiegeData.CardType.Spell)
            {
                indexOfCardSelected = iterator;
                playerCastingSpell.DrawCardWithIndex(indexOfCardSelected);
            }
            iterator++;
        }
    }
}
