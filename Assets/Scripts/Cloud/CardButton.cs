using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardButton : MonoBehaviour, IPointerDownHandler
{
    CardAssigned.Cards cardAssigned;
    public void AssignCard(CardAssigned.Cards cardSent)
    {
        cardAssigned = cardSent;
    }

    public void OnMouseDown()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardCollection.singleton.AddCardToDeck(cardAssigned);
        Debug.Log("card assigned " + cardAssigned);
    }
}
