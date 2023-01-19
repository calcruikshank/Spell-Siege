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
    CardsCollectedForPlayer loadedCollection;

    List<Deck> decks = new List<Deck>();

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
        LoadAllDecks();
        LoadSomeData();
    }


    public void LoadAllDecks()
    {
        LoadDeck0FromCloudSave();
        LoadDeck1FromCloudSave();
        LoadDeck2FromCloudSave();
        LoadDeck3FromCloudSave();
        LoadDeck4FromCloudSave();
        LoadDeck5FromCloudSave();
    }
    public async void SaveDecklists()
    {
        SaveDeckList0();
        SaveDeckList1();
        SaveDeckList2();
        SaveDeckList3();
        SaveDeckList4();
        SaveDeckList5();
    }

    #region decklistsCloudSave
    public async void SaveInitialDecklist0()
    {
        Deck deck0 = new Deck();
        var data = new Dictionary<string, object> { { "Deck0", deck0 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck0FromCloudSave();
    }

    private async void LoadDeck0FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck0" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist0();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck0"]));
    }
    private async void SaveDeckList0()
    {
        var data = new Dictionary<string, object> { { "Deck0", decks[0] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }
    public async void SaveInitialDecklist1()
    {
        Deck deck1 = new Deck();
        var data = new Dictionary<string, object> { { "Deck1", deck1 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck1FromCloudSave();
    }

    private async void LoadDeck1FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck1" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist1();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck1"]));
    }
    private async void SaveDeckList1()
    {
        var data = new Dictionary<string, object> { { "Deck1", decks[1] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }




    public async void SaveInitialDecklist2()
    {
        Deck deck2 = new Deck();
        var data = new Dictionary<string, object> { { "Deck2", deck2 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck2FromCloudSave();
    }

    private async void LoadDeck2FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck2" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist2();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck2"]));
    }
    private async void SaveDeckList2()
    {
        var data = new Dictionary<string, object> { { "Deck2", decks[2] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }



    public async void SaveInitialDecklist3()
    {
        Deck deck3 = new Deck();
        var data = new Dictionary<string, object> { { "Deck3", deck3 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck3FromCloudSave();
    }

    private async void LoadDeck3FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck3" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist3();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck3"]));
    }
    private async void SaveDeckList3()
    {
        var data = new Dictionary<string, object> { { "Deck3", decks[3] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }




    public async void SaveInitialDecklist4()
    {
        Deck deck4 = new Deck();
        var data = new Dictionary<string, object> { { "Deck4", deck4 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck4FromCloudSave();
    }

    private async void LoadDeck4FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck4" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist4();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck4"]));
    }
    private async void SaveDeckList4()
    {
        var data = new Dictionary<string, object> { { "Deck4", decks[4] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }



    public async void SaveInitialDecklist5()
    {
        Deck deck5 = new Deck();
        var data = new Dictionary<string, object> { { "Deck5", deck5 } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        LoadDeck5FromCloudSave();
    }

    private async void LoadDeck5FromCloudSave()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Deck5" });
        if (savedData.Count < 1)
        {
            SaveInitialDecklist5();
            return;
        }
        decks.Add(JsonUtility.FromJson<Deck>(savedData["Deck5"]));
    }
    private async void SaveDeckList5()
    {
        var data = new Dictionary<string, object> { { "Deck5", decks[5] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }



    #endregion







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
            return;
        }
        loadedCollection = JsonUtility.FromJson<CardsCollectedForPlayer>(savedData["CardsCollected"]);

        LoadCardCollection(loadedCollection);

    }


    [SerializeField] Transform cardHolder;

    [SerializeField] Transform angryTurtleButton;

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

        SaveDecklists();
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
