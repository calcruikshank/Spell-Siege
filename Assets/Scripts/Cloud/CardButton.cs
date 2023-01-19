using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardAssigned.Cards cardAssigned;
    Vector3 originalScale;

    TextMeshProUGUI amount;
    public int amountOwned;
    public void AssignCard(CardAssigned.Cards cardSent)
    {
        originalScale = this.transform.localScale;
        cardAssigned = cardSent;
        amount = this.transform.parent.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        amount.gameObject.SetActive(true);
        AddToAmountOwned();
    }

    public void OnMouseDown()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardCollection.singleton.AddCardToDeck(cardAssigned);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.localScale = originalScale * 1.5f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.transform.localScale = originalScale;
    }

    public void AddToAmountOwned()
    {
        amountOwned++;
        amount.text = ("x" + amountOwned);
    }
}
