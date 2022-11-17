using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardInHand : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField]public Transform GameObjectToInstantiate;
    public int indexOfCard;

    public int greenManaCost;
    public int blueManaCost;
    public int whiteManaCost;
    public int blackManaCost;
    public int redManaCost;
    public int genericManaCost;

    public int remainingMana;

    [SerializeField] TextMeshProUGUI greenManaText;
    [SerializeField] TextMeshProUGUI redManaText;
    [SerializeField] TextMeshProUGUI whiteManaText;
    [SerializeField] TextMeshProUGUI blackManaText;
    [SerializeField] TextMeshProUGUI blueManaText;

    [SerializeField] TextMeshProUGUI genericManaText;

    BaseTile.ManaType manaType;

    public Controller playerOwningCard;

    public bool isPurchasable;

    public enum CardType
    {
        Creature,
        Spell,
        Structure
    }
    public CardType cardType;
    // Start is called before the first frame update
    void Start()
    {
        SetCard();
        UpdateMana();
    }

    void SetCard()
    {
        this.blueManaCost = card.blueManaCost;
        this.greenManaCost = card.greenManaCost;
        this.redManaCost = card.redManaCost;
        this.whiteManaCost = card.whiteManaCost;
        this.blackManaCost = card.blackManaCost;
        this.genericManaCost = card.genericManaCost;
    }
    public void UpdateMana()
    {
        if (genericManaCost == 0)
        {
            if (genericManaText != null)
            {
                genericManaText.transform.parent.gameObject.SetActive(false);
            }
        }
        if (greenManaCost == 0)
        {
            greenManaText.transform.parent.gameObject.SetActive(false);
        }
        if (blueManaCost == 0)
        {
            blueManaText.transform.parent.gameObject.SetActive(false);
        }
        if (blackManaCost == 0)
        {
            blackManaText.transform.parent.gameObject.SetActive(false);
        }
        if (whiteManaCost == 0)
        {
            whiteManaText.transform.parent.gameObject.SetActive(false);
        }
        if (redManaCost == 0)
        {
            redManaText.transform.parent.gameObject.SetActive(false);
        }




        /*if (genericManaCost != 0)
        {
            genericManaText.transform.parent.gameObject.SetActive(true);
            genericManaText.text = genericManaCost.ToString();
        }*/
        if (greenManaCost != 0)
        {
            greenManaText.transform.parent.gameObject.SetActive(true);
            greenManaText.text = greenManaCost.ToString();
        }
        if (blueManaCost != 0)
        {
            blueManaText.transform.parent.gameObject.SetActive(true);
            blueManaText.text = blueManaCost.ToString();
        }
        if (blackManaCost != 0)
        {
            blackManaText.transform.parent.gameObject.SetActive(true);
            blackManaText.text = blackManaCost.ToString();
        }
        if (whiteManaCost != 0)
        {
            whiteManaText.transform.parent.gameObject.SetActive(true);
            whiteManaText.text = whiteManaCost.ToString();
        }
        if (redManaCost != 0)
        {
            redManaText.transform.parent.gameObject.SetActive(true);
            redManaText.text = redManaCost.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void CheckToSeeIfPurchasable(PlayerResources resources)
    {
        int tempBlueMana;
        int tempRedMana;
        int tempGreenMana;
        int tempWhiteMana;
        int tempBlackMana;
        int tempGenericMana;

        tempBlueMana = resources.blueMana - blueManaCost;
        tempRedMana = resources.redMana - redManaCost;
        tempGreenMana = resources.greenMana - greenManaCost;
        tempWhiteMana = resources.whiteMana - whiteManaCost;
        tempBlackMana = resources.blackMana - blackManaCost;

        if (tempBlueMana < 0)
        {
            SetToNotPurchasable();
            return;
        }
        if (tempRedMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempGreenMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempWhiteMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempBlackMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        tempGenericMana = tempBlackMana + tempBlueMana + tempGreenMana + tempRedMana + tempRedMana;
        if (tempGenericMana < genericManaCost)
        {
            SetToNotPurchasable();
            return;
        }
        SetToPurchasable();

        /*remainingMana = tempBlueMana + tempRedMana + tempGreenMana + tempWhiteMana + tempBlackMana;
        if (remainingMana >= genericManaCost)
        {
        }*/
    }

    private void SetToPurchasable()
    {
        isPurchasable = true;
    }

    private void SetToNotPurchasable()
    {
        isPurchasable = false;
    }
    GameObject visualVersion;
    private void OnMouseOver()
    {
        if (playerOwningCard.locallySelectedCard != null)
        {
            playerOwningCard.locallySelectedCard.gameObject.SetActive(false);
        }
        if (visualVersion == null && playerOwningCard.locallySelectedCard == null)
        {
            visualVersion = Instantiate(this.gameObject, GameManager.singleton.canvasMain.transform);
            visualVersion.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + .525f);
            visualVersion.transform.localEulerAngles = Vector3.zero;
            visualVersion.transform.localScale = visualVersion.transform.localScale * 2;
            visualVersion.GetComponentInChildren<Collider>().enabled = false;
        }
    }

    private void OnMouseExit()
    {
        if (playerOwningCard.locallySelectedCreature != this)
        {
            if (playerOwningCard.locallySelectedCard != null)
            {
                playerOwningCard.locallySelectedCard.gameObject.SetActive(true);
            }
            TurnOffVisualCard();
        }
    }

    internal void TurnOffVisualCard()
    {
        if (visualVersion != null)
        {
            Destroy(visualVersion);
        }
    }
}
