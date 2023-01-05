using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardButton : MonoBehaviour
{
    CardAssigned.Cards cardAssigned;
    public void AssignCard(CardAssigned.Cards cardSent)
    {
        cardAssigned = cardSent;
    }
}
