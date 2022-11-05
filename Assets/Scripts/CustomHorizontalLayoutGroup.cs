using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizontalLayoutGroup : MonoBehaviour
{
    float spacing;

    int previousNumberOfChildren;
    private void Start()
    {
        CalculateCorrrectTransformsOfChildren();
    }
    private void Update()
    {
        if (transform.childCount != previousNumberOfChildren)
        {
            CalculateCorrrectTransformsOfChildren();
            previousNumberOfChildren = transform.childCount;
        }
    }
    public void CalculateCorrrectTransformsOfChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            float widthOfChild = transform.GetChild(i).transform.GetComponent<RectTransform>().sizeDelta.x;
            spacing = widthOfChild * .2f;
            transform.GetChild(i).localPosition = new Vector3( (i * widthOfChild) + spacing * i, transform.GetChild(i).localPosition.y, transform.GetChild(i).localPosition.z);
            Debug.Log(i);
        }
    }
}
