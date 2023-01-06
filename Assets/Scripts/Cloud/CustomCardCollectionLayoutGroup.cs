using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCardCollectionLayoutGroup : MonoBehaviour
{
    float spacing;

    int numberOfChildren;
    float startingOffset;

    int middleNumber = 0;

    float rotationMultiplier = 5;
    float yOffsetMultiplier = 10;
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
            float widthOfChild = transform.GetChild(i).transform.GetComponent<RectTransform>().sizeDelta.x * transform.GetChild(i).transform.GetComponent<RectTransform>().localScale.x;

            spacing = .2f;
            if (numberOfChildren % 2 != 0)
            {
                startingOffset = (widthOfChild + spacing) * distanceFromMiddleNumber;
            }
            if (numberOfChildren % 2 == 0)
            {
                startingOffset = ((widthOfChild + spacing) * distanceFromMiddleNumber) + (widthOfChild / 2);
            }
        }
    }
}
