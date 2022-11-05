using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizontalLayoutGroup : MonoBehaviour
{
    float spacing;

    int previousNumberOfChildren;
    float startingOffset;
    private void Start()
    {
        CalculateCorrrectTransformsOfChildren();
    }
    private void Update()
    {
        CalculateCorrrectTransformsOfChildren();
    }
    public void CalculateCorrrectTransformsOfChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            float widthOfChild = transform.GetChild(i).transform.GetComponent<RectTransform>().sizeDelta.x * transform.GetChild(i).transform.GetComponent<RectTransform>().localScale.x;
            startingOffset = ((widthOfChild / 2 * transform.childCount) - widthOfChild / 2);
            spacing = widthOfChild * .3f;
            startingOffset = startingOffset +  widthOfChild / 2 + spacing;
            transform.GetChild(i).localPosition = new Vector3( ((i * widthOfChild) + spacing * i) - startingOffset, transform.GetChild(i).localPosition.y, transform.GetChild(i).localPosition.z);
        }
    }
}
