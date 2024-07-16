using System;
using System.Collections.Generic;


[Serializable]
public class Deck 
{
    public string deckName; 
    public List<int> deck = new List<int>();

    internal int GetAmountOfCard(SpellSiegeData.Cards cardAssigned)
    {
        int amountInDeck = 0;
        int cardId = (int)cardAssigned; // Assuming the cardAssigned can be cast to int

        foreach (int card in deck)
        {
            if (card == cardId)
            {
                amountInDeck++;
            }
        }

        return amountInDeck;
    }
}
