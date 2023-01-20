using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardIconInDeck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    Transform visualCardOnHover;
    public CardAssigned.Cards c;
    Canvas mainCanvas;

    [SerializeField] Image cardImage;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI numOfTypesInDeck;

    int numberInDeck = 0;
    void Awake()
    {
        mainCanvas = FindObjectOfType<Canvas>();
    }

    public void AddNumber()
    {
        numberInDeck++;
        numOfTypesInDeck.text = ("X" + numberInDeck);
    }
    public void SubtractNumber()
    {
        numberInDeck--;
        numOfTypesInDeck.text = ("X" + numberInDeck);
    }
    public void InjectDependencies(CardAssigned.Cards c)
    {
        this.c = c;
        CardInHand selectedCard = CardCollectionData.singleton.GetCardAssociatedWithType(c);

        visualCardOnHover = Instantiate(selectedCard, mainCanvas.transform).transform;

        this.cardImage.sprite = visualCardOnHover.GetChild(0).GetChild(0).GetComponent<Image>().sprite;

        title.text = selectedCard.cardTitle.text;
        visualCardOnHover.gameObject.SetActive(false);
        numberInDeck++;
        numOfTypesInDeck.text = ("X" + numberInDeck);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (numberInDeck == 1)
        {
            CardCollection.singleton.RemoveCardFromDeck(this);
            CardCollection.singleton.DestroyCardIcon(this);
            visualCardOnHover.gameObject.SetActive(false);
        }
        if (numberInDeck > 1)
        {
            CardCollection.singleton.RemoveCardFromDeck(this);
            SubtractNumber();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        visualCardOnHover.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        visualCardOnHover.gameObject.SetActive(false);
    }

    void Update()
    {
        if (visualCardOnHover != null)
        {
            visualCardOnHover.position = new Vector3( this.transform.position.x - 22, this.transform.position.y, this.transform.position.z);
        }
    }
}
