using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;

public class OpenPackManager : MonoBehaviour
{

    [SerializeField] Transform cardVisualParent;
    [SerializeField] Transform coreSetPack;

    //todo take in a pack type "expansion"
    public async void TempOpenPack()
    {
        Instantiate(coreSetPack, new Vector3(0, 1.2f, 0), Quaternion.identity);
        Debug.Log("Pressed");
        //TODO add drop rates for each card based on rarity

        //The first card is a guaranteed rare.
        //get a random number from 1 to 100 if that number is lower than 1 upgrades to a legendary


        if (CardCollectionData.singleton.loadedCollection == null)
        {
            CardCollectionData.singleton.LoadSomeData();
        }
        List<int> cardsOpened = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            SpellSiegeData.cardRarity rarity = GetRarity();
            SpellSiegeData.Cards cardGrabbed = CardCollectionData.singleton.GetCardAssociatedWithRarity(rarity);
            //cardsOpened.Add(UnityEngine.Random.Range(0, (int)SpellSiegeData.Cards.NumOfCardTypes));
            Debug.Log(cardGrabbed + ": with the rarity of " + rarity);
            cardsOpened.Add((int)cardGrabbed);
            InstantiateCardVisual(cardGrabbed);
        }
        foreach (int i in cardsOpened)
        {
            CardCollectionData.singleton.loadedCollection.cardsCollected.Add(i);
        }
        var data = new Dictionary<string, object> { { "CardsCollected", CardCollectionData.singleton.loadedCollection } };
        await CloudSaveService.Instance.Data.ForceSaveAsync(data);


        //Todo subtract from total packs 
    }

    private void InstantiateCardVisual(SpellSiegeData.Cards cardGrabbed)
    {
        CardInHand cardGO = CardCollectionData.singleton.GetCardAssociatedWithType(cardGrabbed);
        
        GameObject instantiatedCard = Instantiate(cardGO, cardVisualParent).gameObject;
        Destroy(instantiatedCard.GetComponent<CardInHand>());
    }


    public SpellSiegeData.cardRarity GetRarity()
    {
        float randomNumber = UnityEngine.Random.Range(0f, 100f);
        if (randomNumber < 1f)
        {
            return SpellSiegeData.cardRarity.Legendary;
        }
        if (randomNumber < 10)
        {
            return SpellSiegeData.cardRarity.mythic;
        }
        if (randomNumber < 25)
        {
            return SpellSiegeData.cardRarity.rare;
        }
        if (randomNumber < 40)
        {
            return SpellSiegeData.cardRarity.uncommon;
        }
        return SpellSiegeData.cardRarity.common;
    }
}
