using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System;
using System.Linq;
using TMPro;

public class CardCollection : MonoBehaviour
{
    public static CardCollection singleton;
    Deck currentSelectedDeck;

    [SerializeField] TextMeshProUGUI loadedDeckName;

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
    }
    private void Start()
    {
        LoadCardCollection(CardCollectionData.singleton.loadedCollection);
    }




    [SerializeField] Transform cardHolder;

    [SerializeField] TextMeshProUGUI[] deckNames;


    public int lastInstantiatedInt = 0;

    public List<SpellSiegeData.Cards> cardsInstantiatedInCollection = new List<SpellSiegeData.Cards>();
    public List<CardButton> instantiatedCardButtons = new List<CardButton>();
    private void LoadCardCollection(CardsCollectedForPlayer myObject)
    {
        for (int i = 0; i < CardCollectionData.singleton.decks.Length; i++)
        {
            deckNames[i].text = CardCollectionData.singleton.decks[i].deckName;
        }
        for (int i = lastInstantiatedInt; i < myObject.cardsCollected.Count; i++)
        {
            AssignButtonToTransform(CardCollectionData.singleton.GetCardAssociatedWithType((SpellSiegeData.Cards)myObject.cardsCollected[i]), myObject.cardsCollected.Count);
        }
        lastInstantiatedInt = myObject.cardsCollected.Count;
    }

    [SerializeField] TextMeshProUGUI amountInCollectionTextPrefab;
    private void AssignButtonToTransform(CardInHand transformSent, int amountincollection)
    {
        if (!cardsInstantiatedInCollection.Contains(transformSent.cardAssignedToObject))
        {
            Transform instantiatedObject = Instantiate(transformSent.transform, cardHolder.transform.GetChild(cardsInstantiatedInCollection.Count).transform);
            CardInHand cardToAssign = instantiatedObject.gameObject.GetComponent<CardInHand>();
            cardsInstantiatedInCollection.Add(cardToAssign.cardAssignedToObject);
            CardButton newCardButton = instantiatedObject.gameObject.AddComponent<CardButton>();
            newCardButton.AssignCard(cardToAssign.cardAssignedToObject);
            instantiatedCardButtons.Add(newCardButton);
            Destroy(cardToAssign);
            newCardButton.amount = Instantiate(amountInCollectionTextPrefab, instantiatedObject);
        }
        else
        {
            foreach (CardButton cb in instantiatedCardButtons)
            {
                if (cb.cardAssigned == transformSent.cardAssignedToObject)
                {
                    cb.AddToAmountOwned();
                }
            }
        }
    }


    public void ChooseDeck(int deckChosen)
    {
        currentSelectedDeck = CardCollectionData.singleton.decks[deckChosen];
        LoadPanelWithCurrentlySelectedDeck(currentSelectedDeck);
    }

    [SerializeField] Transform loadedDeckPanel;

    [SerializeField] TMP_InputField nameInputField;
    private void LoadPanelWithCurrentlySelectedDeck(Deck currentSelectedDeck)
    {
        loadedDeckPanel.gameObject.SetActive(true);
        deckSelectionPanel.gameObject.SetActive(false);

        loadedDeckName.text = currentSelectedDeck.deckName;
        nameInputField.text = currentSelectedDeck.deckName;
        Debug.Log(currentSelectedDeck.deckName);
        LoadDeck(currentSelectedDeck);
    }

    List<GameObject> instantiatedCardsInDeck = new List<GameObject>();
    private void LoadDeck(Deck currentSelectedDeckSent)
    {
        foreach (GameObject go in instantiatedCardsInDeck)
        {
            Destroy(go);
        }
        instantiatedCardsInDeck.Clear();
        foreach (SpellSiegeData.Cards c in currentSelectedDeckSent.deck)
        {
            InstantiateCardPrefabInCurrentSelectedDeck(c);
        }
    }

    [SerializeField] Transform deckSelectionPanel;
    public void GoToDeckSelectionPanel()
    {
        currentSelectedDeck.deckName = loadedDeckName.text.ToString();
        currentSelectedDeck = null;
        loadedDeckPanel.gameObject.SetActive(false);
        deckSelectionPanel.gameObject.SetActive(true);

        
        CardCollectionData.singleton.SaveDecklists();

        for (int i = 0; i < CardCollectionData.singleton.decks.Length; i++)
        {
            deckNames[i].text = CardCollectionData.singleton.decks[i].deckName;
        }
    }


    [SerializeField] GameObject cardInDeckIcon;
    [SerializeField] Transform loadedDeckVertScrollRect;
    internal void AddCardToDeck(SpellSiegeData.Cards cardAssigned, int amountOwned)
    {
        if (currentSelectedDeck != null)
        {
            if (currentSelectedDeck.GetAmountOfCard(cardAssigned) < amountOwned && currentSelectedDeck.GetAmountOfCard(cardAssigned) < 4)
            {
                InstantiateCardPrefabInCurrentSelectedDeck(cardAssigned);
                currentSelectedDeck.deck.Add((int)cardAssigned);
            }
        }
    }


    internal void RemoveCardFromDeck(CardIconInDeck cardIconInDeck)
    {
        currentSelectedDeck.deck.Remove((int)cardIconInDeck.c);
    }

    public void DestroyCardIcon(CardIconInDeck cardIconInDeck)
    {
        instantiatedCardsInDeck.Remove(cardIconInDeck.gameObject);
        Destroy(cardIconInDeck.gameObject);
    }
    private void InstantiateCardPrefabInCurrentSelectedDeck(SpellSiegeData.Cards cardAssigned)
    {
        bool gameObjectHasBeenInstantiated = false;
        if (instantiatedCardsInDeck.Count > 0)
        {
            foreach (GameObject instCard in instantiatedCardsInDeck)
            {
                if (instCard.GetComponent<CardIconInDeck>().c == cardAssigned)
                {
                    gameObjectHasBeenInstantiated = true;
                    instCard.GetComponent<CardIconInDeck>().AddNumber();
                }
            }
        }
        if (!gameObjectHasBeenInstantiated)
        {
            GameObject instCard = Instantiate(cardInDeckIcon, loadedDeckVertScrollRect.transform);
            instCard.GetComponent<CardIconInDeck>().InjectDependencies(cardAssigned);
            instantiatedCardsInDeck.Add(instCard);
        }
    }
}
