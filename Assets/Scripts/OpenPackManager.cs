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
    public void TempOpenPack()
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
            InstantiateCardVisual(cardGrabbed, i);
        }
        foreach (int i in cardsOpened)
        {
            CardCollectionData.singleton.loadedCollection.cardsCollected.Add(i);

        }
        var data = new Dictionary<string, object> { { "CardsCollected", CardCollectionData.singleton.loadedCollection } };
        CloudSaveService.Instance.Data.ForceSaveAsync(data);


        //Todo subtract from total packs 
    }
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float movementDuration = 0.2f; // Adjust duration for faster movement

    private void InstantiateCardVisual(SpellSiegeData.Cards cardGrabbed, int position)
    {
        CardInHand cardGO = CardCollectionData.singleton.GetCardAssociatedWithType(cardGrabbed);

        GameObject instantiatedCard = Instantiate(cardGO, cardVisualParent).gameObject;

        Destroy(instantiatedCard.GetComponent<CardInHand>());

        // Start the coroutine to move the card to the target point with animation
        StartCoroutine(MoveCardToPoint(instantiatedCard, targetPoints[position].position, movementDuration));
    }


    public List<Transform> targetPoints; // List of target points
    private IEnumerator MoveCardToPoint(GameObject card, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = card.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = movementCurve.Evaluate(t);
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the card reaches the target position at the end
        card.transform.position = targetPosition;

        // Optional: Add a jolt effect
        StartCoroutine(JoltEffect(card, 0.5f, 0.01f)); // Adjust magnitude and duration for the jolt effect
    }

    private IEnumerator JoltEffect(GameObject card, float magnitude, float duration)
    {
        Vector3 originalPosition = card.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float xOffset = UnityEngine.Random.Range(-magnitude, magnitude);
            float yOffset = UnityEngine.Random.Range(-magnitude, magnitude);
            card.transform.position = originalPosition + new Vector3(xOffset, yOffset, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the card returns to the original position
        card.transform.position = originalPosition;
    }




    public SpellSiegeData.cardRarity GetRarity()
    {
        float randomNumber = UnityEngine.Random.Range(0f, 100f);
        if (randomNumber < 2f)
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
