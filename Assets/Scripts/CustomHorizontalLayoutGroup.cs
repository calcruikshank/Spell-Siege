using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizontalLayoutGroup : MonoBehaviour
{
    float spacing;

    int numberOfChildren;
    float startingOffset;

    int middleNumber = 0;

    float rotationMultiplier = 2;
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
            Transform transformToMove = transform.GetChild(i);

            if (Vector3.Distance(transformToMove.localPosition, new Vector3(startingOffset, yOffsetMultiplier * -Mathf.Abs(distanceFromMiddleNumber), transformToMove.localPosition.z)) > .1f)
            {
                transformToMove.localPosition = Vector3.MoveTowards(transformToMove.localPosition, new Vector3(startingOffset, yOffsetMultiplier * -Mathf.Abs(distanceFromMiddleNumber), transformToMove.localPosition.z), 1000 * Time.deltaTime);
            }
            transformToMove.localRotation = Quaternion.RotateTowards(transformToMove.localRotation, new Quaternion(0, 0,0 +( -rotationMultiplier * distanceFromMiddleNumber * Mathf.PI / 180),  1), 1000 * Time.deltaTime);
           
        }
    }

}
