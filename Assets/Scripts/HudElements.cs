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
    }

    public void UpdateDrawSlider(float valueSent)
    {
        drawCardSlider.value = valueSent;
    }
}
