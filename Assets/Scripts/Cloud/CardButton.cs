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
        foreach (Image i in GetComponentsInChildren<Image>())
        {
            i.enabled = false;
            i.raycastTarget = false;
        }
    }

    public void OnMouseDown()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardCollection.singleton.CardHasBeenClicked(cardAssigned);
        Debug.Log("card assigned " + cardAssigned);
    }
}
