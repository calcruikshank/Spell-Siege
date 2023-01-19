using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System;
using System.Linq;

public class CardCollection : MonoBehaviour
{
    public static CardCollection singleton;
    Deck currentSelectedDeck;

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


    public int lastInstantiatedInt = 0;

    public List<CardAssigned.Cards> cardsInstantiatedInCollection = new List<CardAssigned.Cards>();
    public List<CardButton> instantiatedCardButtons = new List<CardButton>();
    private void LoadCardCollection(CardsCollectedForPlayer myObject)
    {
        for (int i = lastInstantiatedInt; i < myObject.cardsCollected.Count; i++)
        {
            AssignButtonToTransform(GetCardAssociatedWithType((CardAssigned.Cards)myObject.cardsCollected[i]), i);
        }
        lastInstantiatedInt = myObject.cardsCollected.Count;
    }

    private void AssignButtonToTransform(CardInHand transformSent, int i)
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




    [SerializeField] List<CardInHand> allCardsInGame = new List<CardInHand>();
    public CardInHand GetCardAssociatedWithType(CardAssigned.Cards cardGrabbed)
    {
        CardInHand selectedObject;

        selectedObject = allCardsInGame.FirstOrDefault(s => s.cardAssignedToObject == cardGrabbed);

        if (selectedObject == null)
        {
            //Debug.LogError("Could not find prefab associated with -> " + buildingType + " defaulting to 0"); 
            //TODO when we have enough building prefabs created we can uncomment this to figure out what we're missing
            selectedObject = allCardsInGame[0];
        }
        return selectedObject;
    }




    public async void TempOpenPack()
    {
        if (CardCollectionData.singleton.loadedCollection == null)
        {
            CardCollectionData.singleton.LoadSomeData();
        }
        List<int> cardsOpened = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            cardsOpened.Add(UnityEngine.Random.Range(0, (int)CardAssigned.Cards.NumOfCardTypes));
        }
        foreach (int i in cardsOpened)
        {
            CardCollectionData.singleton.loadedCollection.cardsCollected.Add(i);
        }
        var data = new Dictionary<string, object> { { "CardsCollected", CardCollectionData.singleton.loadedCollection } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadCardCollection(CardCollectionData.singleton.loadedCollection);
    }

    public void ChooseDeck(int deckChosen)
    {
        currentSelectedDeck = CardCollectionData.singleton.decks[deckChosen];
        LoadPanelWithCurrentlySelectedDeck(currentSelectedDeck);
    }

    [SerializeField] Transform loadedDeckPanel;
    private void LoadPanelWithCurrentlySelectedDeck(Deck currentSelectedDeck)
    {
        loadedDeckPanel.gameObject.SetActive(true);
        deckSelectionPanel.gameObject.SetActive(false);

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
        foreach (CardAssigned.Cards c in currentSelectedDeckSent.deck)
        {
            InstantiateCardPrefabInCurrentSelectedDeck(c);
        }
    }

    [SerializeField] Transform deckSelectionPanel;
    public void GoToDeckSelectionPanel()
    {
        currentSelectedDeck = null;
        loadedDeckPanel.gameObject.SetActive(false);
        deckSelectionPanel.gameObject.SetActive(true);

        CardCollectionData.singleton.SaveDecklists();
    }


    [SerializeField] GameObject cardInDeckIcon;
    [SerializeField] Transform loadedDeckVertScrollRect;
    internal void AddCardToDeck(CardAssigned.Cards cardAssigned)
    {
        if (currentSelectedDeck != null)
        {
            InstantiateCardPrefabInCurrentSelectedDeck(cardAssigned);
            currentSelectedDeck.deck.Add((int)cardAssigned);
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
    private void InstantiateCardPrefabInCurrentSelectedDeck(CardAssigned.Cards cardAssigned)
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
