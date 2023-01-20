using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.CloudSave;
using UnityEngine;

public class CardCollectionData : MonoBehaviour
{
    public static CardCollectionData singleton;
    // Start is called before the first frame update

    public Deck[] decks = new Deck[6];
    private void Awake()
    {
        decks = new Deck[6];
        if (singleton != null)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        singleton = this;
    }
    void Start()
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
    public void SaveDecklists()
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
        deck0.deckName = "Deck 1";
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
        decks[0] = (JsonUtility.FromJson<Deck>(savedData["Deck0"]));
    }
    private async void SaveDeckList0()
    {
        var data = new Dictionary<string, object> { { "Deck0", decks[0] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }
    public async void SaveInitialDecklist1()
    {
        Deck deck1 = new Deck();
        deck1.deckName = "Deck 2";
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
        decks[1] = (JsonUtility.FromJson<Deck>(savedData["Deck1"]));
    }
    private async void SaveDeckList1()
    {
        var data = new Dictionary<string, object> { { "Deck1", decks[1] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }




    public async void SaveInitialDecklist2()
    {
        Deck deck2 = new Deck();
        deck2.deckName = "Deck 3";
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
        decks[2] = (JsonUtility.FromJson<Deck>(savedData["Deck2"]));
    }
    private async void SaveDeckList2()
    {
        var data = new Dictionary<string, object> { { "Deck2", decks[2] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }



    public async void SaveInitialDecklist3()
    {
        Deck deck3 = new Deck();
        deck3.deckName = "Deck 4";
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
        decks[3] = (JsonUtility.FromJson<Deck>(savedData["Deck3"]));
    }
    private async void SaveDeckList3()
    {
        var data = new Dictionary<string, object> { { "Deck3", decks[3] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }




    public async void SaveInitialDecklist4()
    {
        Deck deck4 = new Deck();
        deck4.deckName = "Deck 5";
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
        decks[4] = (JsonUtility.FromJson<Deck>(savedData["Deck4"]));
        
    }
    private async void SaveDeckList4()
    {
        var data = new Dictionary<string, object> { { "Deck4", decks[4] } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
    }



    public async void SaveInitialDecklist5()
    {
        Deck deck5 = new Deck();
        deck5.deckName = "Deck 6";
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
        decks[5] = (JsonUtility.FromJson<Deck>(savedData["Deck5"]));
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


    public CardsCollectedForPlayer loadedCollection;
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

}
