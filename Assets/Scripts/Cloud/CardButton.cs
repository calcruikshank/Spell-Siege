using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SpellSiegeData.Cards cardAssigned;
    Vector3 originalScale;

    public TextMeshProUGUI amount;
    public int amountOwned;
    public void AssignCard(SpellSiegeData.Cards cardSent)
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
        CardCollection.singleton.AddCardToDeck(cardAssigned, amountOwned);
    }

    Vector3 originalAmountPosition;
    Vector3 originalAmountScale;
    public void OnPointerEnter(PointerEventData eventData)
    {
        originalAmountPosition = amount.transform.position;
        originalAmountScale = amount.transform.localScale;
        this.transform.localScale = originalScale * 1.5f;
        amount.transform.parent = FindObjectOfType<Canvas>().transform;
    }
    void Update()
    {
        if (amount != null)
        {
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        amount.transform.parent = this.transform;
        amount.transform.localPosition = new Vector3(0, amount.transform.localPosition.y, amount.transform.localPosition.z);
        this.transform.localScale = originalScale;
    }

    public void AddToAmountOwned()
    {
        amountOwned++;
        amount.text = ("x" + amountOwned);
    }
}
