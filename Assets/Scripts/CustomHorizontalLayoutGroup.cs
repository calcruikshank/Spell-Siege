using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizontalLayoutGroup : MonoBehaviour
{
    float spacing;

    int numberOfChildren;
    float startingOffset;

    int middleNumber = 0;
    private void Update()
    {
        CalculateCorrrectTransformsOfChildren();
    }
    public void CalculateCorrrectTransformsOfChildren()
    {
        numberOfChildren = transform.childCount;
        for (int i = 0; i < numberOfChildren; i++)
        {
            if (numberOfChildren % 2 == 0)
            {
                middleNumber = numberOfChildren / 2;
            }
            if (numberOfChildren % 2 != 0)
            {
                middleNumber = (numberOfChildren - 1) / 2;
            }
            int distanceFromMiddleNumber = i - middleNumber;
            Debug.Log(distanceFromMiddleNumber);
            float widthOfChild = transform.GetChild(i).transform.GetComponent<RectTransform>().sizeDelta.x * transform.GetChild(i).transform.GetComponent<RectTransform>().localScale.x;
            
            spacing = widthOfChild * .3f; 
            if (numberOfChildren % 2 != 0)
            {
                startingOffset = (widthOfChild + spacing) * distanceFromMiddleNumber;
            }
            if (numberOfChildren % 2 == 0)
            {
                startingOffset = ((widthOfChild + spacing) * distanceFromMiddleNumber) + (widthOfChild / 2);
            }
            transform.GetChild(i).localPosition = new Vector3( startingOffset, transform.GetChild(i).localPosition.y, transform.GetChild(i).localPosition.z);
            transform.GetChild(i).localEulerAngles = new Vector3();
        }
    }
}
