using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System;

public class CardCollection : MonoBehaviour
{
    CardsCollectedForPlayer cardsCollected;
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
        CardsCollectedForPlayer myObject = JsonUtility.FromJson<CardsCollectedForPlayer>(savedData["CardsCollected"]);
        Debug.Log(myObject.AngryTurtle);

        LoadCardCollection(myObject);

    }

    [SerializeField] Transform angryTurtleButton;

    private void LoadCardCollection(CardsCollectedForPlayer myObject)
    {
        if (myObject.AngryTurtle > 0)
        {
        }
    }

}
