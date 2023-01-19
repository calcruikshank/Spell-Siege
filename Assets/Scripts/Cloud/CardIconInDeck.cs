using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardIconInDeck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    Transform visualCardOnHover;
    public CardAssigned.Cards c;
    public void InjectDependencies(CardAssigned.Cards c)
    {
        this.c = c;
        CardInHand selectedCard = CardCollection.singleton.GetCardAssociatedWithType(c);

        visualCardOnHover = Instantiate(selectedCard, this.transform).transform;
        visualCardOnHover.gameObject.SetActive(false);


    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardCollection.singleton.RemoveCardFromDeck(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        visualCardOnHover.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        visualCardOnHover.gameObject.SetActive(false);
    }
}
