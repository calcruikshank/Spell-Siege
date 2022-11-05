using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour
{
    [SerializeField] List<CardInHand> allCardsInGame;
    [SerializeField] List<CardInHand> instantiatedCardInHand;
    [SerializeField] Transform content;

    private void Start()
    {
        for (int i = 0; i < allCardsInGame.Count; i++)
        {
            allCardsInGame[i].enabled = true;
            GameObject instantiatedCard = Instantiate(allCardsInGame[i].gameObject, content.GetChild(i));
            instantiatedCardInHand.Add(instantiatedCard.GetComponent<CardInHand>());

            
            Destroy(instantiatedCard.GetComponent<CardInHand>());

        }
    }
}
