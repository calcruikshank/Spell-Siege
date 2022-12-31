using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudElements : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI whiteMana;
    [SerializeField] TextMeshProUGUI blackMana;
    [SerializeField] TextMeshProUGUI blueMana;
    [SerializeField] TextMeshProUGUI greenMana;
    [SerializeField] TextMeshProUGUI redMana;
    [SerializeField] TextMeshProUGUI whiteManaCap;
    [SerializeField] TextMeshProUGUI blackManaCap;
    [SerializeField] TextMeshProUGUI blueManaCap;
    [SerializeField] TextMeshProUGUI greenManaCap;
    [SerializeField] TextMeshProUGUI redManaCap;
    [SerializeField] Slider drawCardSlider;
    [SerializeField] public Transform cardParent;

    public void UpdateHudVisuals(Controller playerSent, float maxDrawValue)
    {
        this.drawCardSlider.fillRect.GetComponent<Image>().color = playerSent.transparentCol;
        drawCardSlider.maxValue = maxDrawValue;
    }
    public void UpdateHudElements(PlayerResources playerResources)
    {
        blueMana.text = playerResources.blueMana.ToString();
        blackMana.text = playerResources.blackMana.ToString();
        whiteMana.text = playerResources.whiteMana.ToString();
        redMana.text = playerResources.redMana.ToString();
        greenMana.text = playerResources.greenMana.ToString();



        blueManaCap.text = playerResources.blueManaCap.ToString();
        blackManaCap.text = playerResources.blackManaCap.ToString();
        whiteManaCap.text = playerResources.whiteManaCap.ToString();
        redManaCap.text = playerResources.redManaCap.ToString();
        greenManaCap.text = playerResources.greenManaCap.ToString();



        /*if (playerResources.blackManaCap <= 0)
        {
            blackManaCap.gameObject.SetActive(false);
        }
        if (playerResources.blackManaCap > 0)
        {
            blackManaCap.gameObject.SetActive(true);
        }


        if (playerResources.redManaCap <= 0)
        {
            redManaCap.gameObject.SetActive(false);
        }
        if (playerResources.redManaCap > 0)
        {
            redManaCap.gameObject.SetActive(true);
        }


        if (playerResources.whiteManaCap <= 0)
        {
            whiteManaCap.gameObject.SetActive(false);
        }
        if (playerResources.whiteManaCap > 0)
        {
            whiteManaCap.gameObject.SetActive(true);
        }


        if (playerResources.greenManaCap <= 0)
        {
            greenManaCap.gameObject.SetActive(false);
        }
        if (playerResources.greenManaCap > 0)
        {
            greenManaCap.gameObject.SetActive(true);
        }


        if (playerResources.blueManaCap <= 0)
        {
            blueManaCap.gameObject.SetActive(false);
        }
        if (playerResources.blueManaCap > 0)
        {
            blueManaCap.gameObject.SetActive(true);
        }
        if (playerResources.blackManaCap <= 0)
        {
            blackManaCap.gameObject.SetActive(false);
        }
        if (playerResources.blackManaCap > 0)
        {
            blackManaCap.gameObject.SetActive(true);
        }




        if (playerResources.blackMana <= 0)
        {
            blackMana.gameObject.SetActive(false);
        }
        if (playerResources.blackMana > 0)
        {
            blackMana.gameObject.SetActive(true);
        }



        if (playerResources.redMana <= 0)
        {
            redMana.gameObject.SetActive(false);
        }
        if (playerResources.redMana > 0)
        {
            redMana.gameObject.SetActive(true);
        }


        if (playerResources.whiteMana <= 0)
        {
            whiteMana.gameObject.SetActive(false);
        }
        if (playerResources.whiteMana > 0)
        {
            whiteMana.gameObject.SetActive(true);
        }


        if (playerResources.greenMana <= 0)
        {
            greenMana.gameObject.SetActive(false);
        }
        if (playerResources.greenMana > 0)
        {
            greenMana.gameObject.SetActive(true);
        }


        if (playerResources.blueMana <= 0)
        {
            blueMana.gameObject.SetActive(false);
        }
        if (playerResources.blueMana > 0)
        {
            blueMana.gameObject.SetActive(true);
        }*/
    }

    public void UpdateDrawSlider(float valueSent)
    {
        drawCardSlider.value = valueSent;
    }
}
