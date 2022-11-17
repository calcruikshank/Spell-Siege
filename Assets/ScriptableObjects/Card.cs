using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")] 
public class Card : ScriptableObject
{
    public string nameOfCard;

    public int greenManaCost;
    public int blueManaCost;
    public int whiteManaCost;
    public int blackManaCost;
    public int redManaCost;
    public int genericManaCost;
}
