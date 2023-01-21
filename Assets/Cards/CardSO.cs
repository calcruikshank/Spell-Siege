using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardSO : ScriptableObject
{
    public SpellSiegeData.Cards cardAssigned;
    public string cardName;
    public GameObject gameObjectToInstantiate;
    public int attack;
    public int health;
    public int blueManaCost;
    public int redManaCost;
    public int whiteManaCost;
    public int blackManaCost;
    public int greenManaCost;
    public int genericManaCost;
    public Sprite cardArt;
    public string cardText;
    public SpellSiegeData.cardRarity rarity;
    public SpellSiegeData.CardType cardType;
    public SpellSiegeData.travType traversableType;
    public SpellSiegeData.CreatureType creatureType;
}
