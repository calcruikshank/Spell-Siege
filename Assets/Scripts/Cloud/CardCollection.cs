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
        SaveSomeData();
        LoadSomeData();
    }

    public async void GrabCardsCollected()
    {
        
    }

    private void SetNewPlayer()
    {
    }

    public async void SaveSomeData()
    { 
        var data = new Dictionary<string, object> { { AuthenticationService.Instance.PlayerId, new CardsCollectedForPlayer()} };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        List<string> keys = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();
        for (int i = 0; i < keys.Count; i++)
        {
            Debug.Log(keys[i]);
        }
    }
    public async void LoadSomeData()
    {
        Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { AuthenticationService.Instance.PlayerId });
        Debug.Log(savedData[AuthenticationService.Instance.PlayerId]);
    }
}
