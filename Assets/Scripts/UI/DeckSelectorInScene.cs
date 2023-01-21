using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckSelectorInScene : MonoBehaviour
{
    [SerializeField] Transform deckSelectors;
    [SerializeField] Transform showBattlefieldButton;
    [SerializeField] Transform showCardsButton;
    [SerializeField] Transform chooseDeckText;

    [SerializeField] Transform[] decks;

    bool hasAddedBlack = false;
    bool hasAddedBlue = false;
    bool hasAddedRed = false;
    bool hasAddedGreen = false;
    bool hasAddedWhite = false;

    public Deck SelectedDeck;

    public Controller localPlayerInScene;

    public static DeckSelectorInScene singleton;


    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
    }
    public void Start()
    {
        for (int i = 0; i < decks.Length; i++)
        {
            //oof
            decks[i].GetComponentInChildren<TextMeshProUGUI>().text = CardCollectionData.singleton.decks[i].deckName;
            for (int j = 0; j < CardCollectionData.singleton.decks[i].deck.Count; j++)
            {
                CardInHand cardInDeck = CardCollectionData.singleton.GetCardAssociatedWithType((SpellSiegeData.Cards)CardCollectionData.singleton.decks[i].deck[j]);

                if (cardInDeck.blackManaCost > 0)
                {
                    decks[i].GetComponentInChildren<IndividualDeckSelectorPrefab>().blackMana.gameObject.SetActive(true);
                    hasAddedBlack = true;
                }
                if (cardInDeck.blueManaCost > 0)
                {
                    decks[i].GetComponentInChildren<IndividualDeckSelectorPrefab>().blue.gameObject.SetActive(true);
                    hasAddedBlue = true;
                }
                if (cardInDeck.redManaCost > 0)
                {
                    decks[i].GetComponentInChildren<IndividualDeckSelectorPrefab>().red.gameObject.SetActive(true);
                    hasAddedRed = true;
                }
                if (cardInDeck.greenManaCost > 0)
                {
                    decks[i].GetComponentInChildren<IndividualDeckSelectorPrefab>().green.gameObject.SetActive(true);
                    hasAddedGreen = true;
                }
                if (cardInDeck.whiteManaCost > 0)
                {
                    decks[i].GetComponentInChildren<IndividualDeckSelectorPrefab>().white.gameObject.SetActive(true);
                    hasAddedWhite = true;
                }
            }
        }
    }
    public void ShowBattlefield()
    {
        deckSelectors.gameObject.SetActive(false);
        showBattlefieldButton.gameObject.SetActive(false);
        chooseDeckText.gameObject.SetActive(false);
        showCardsButton.gameObject.SetActive(true);

    }
    public void ShowDecks()
    {
        deckSelectors.gameObject.SetActive(true);
        showBattlefieldButton.gameObject.SetActive(true);
        showCardsButton.gameObject.SetActive(false);
        chooseDeckText.gameObject.SetActive(true);
    }
    public void ChooseDeck(int deckChosen)
    {
        SelectedDeck = CardCollectionData.singleton.decks[deckChosen];
        this.gameObject.SetActive(false);

        string json = JsonUtility.ToJson(SelectedDeck);
        localPlayerInScene.ChooseDeckServerRpc(json);
    }

    public void AssignLocalPlayer(Controller playerSent)
    {
        localPlayerInScene = playerSent;
    }
}
