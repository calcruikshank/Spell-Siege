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
    CardsCollectedForPlayer cardsCollected;
    CardsCollectedForPlayer loadedCollection;

    [SerializeField] List<Deck> decks = new List<Deck>();

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
        LoadSomeData();
    }

    private void SetNewPlayer()
    {
    }

    public async void SaveInitialCardsCollected()
    {
        CardsCollectedForPlayer baseCardsCollectedForPlayer = new CardsCollectedForPlayer();
        var data = new Dictionary<string, object> { { "CardsCollected", baseCardsCollectedForPlayer } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadSomeData();
    }
    public async void LoadSomeData()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "CardsCollected" });

        if (savedData.Count < 1)
        {
            SaveInitialCardsCollected();
            Debug.Log("Loading card collection for the first time");
        }
        loadedCollection = JsonUtility.FromJson<CardsCollectedForPlayer>(savedData["CardsCollected"]);

        LoadCardCollection(loadedCollection);

    }


    [SerializeField] Transform cardHolder;

    [SerializeField] Transform angryTurtleButton;

    public int lastInstantiatedInt = 0;
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
        Transform instantiatedObject = Instantiate(transformSent.transform, cardHolder.transform.GetChild(i).transform);
        CardInHand cardToAssign = instantiatedObject.gameObject.GetComponent<CardInHand>();
        CardButton newCardButton = instantiatedObject.gameObject.AddComponent<CardButton>();
        newCardButton.AssignCard(cardToAssign.cardAssignedToObject);
        Destroy(cardToAssign);
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
        if (loadedCollection == null)
        {
            LoadSomeData();
        }
        List<int> cardsOpened = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            cardsOpened.Add(UnityEngine.Random.Range(0, (int)CardAssigned.Cards.NumOfCardTypes));
        }
        foreach (int i in cardsOpened)
        {
            loadedCollection.cardsCollected.Add(i);
        }
        var data = new Dictionary<string, object> { { "CardsCollected", loadedCollection } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadCardCollection(loadedCollection);
    }

    public void ChooseDeck(int deckChosen)
    {
        currentSelectedDeck = decks[deckChosen];
    }


    internal void CardHasBeenClicked(CardAssigned.Cards cardAssigned)
    {
        if (currentSelectedDeck != null)
        {
            currentSelectedDeck.deck.Add(cardAssigned);
        }
    }
}
