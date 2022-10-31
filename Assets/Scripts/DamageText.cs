using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private static int sortingOrder;
    private TextMeshPro textMesh;
    private float dissapearTimer;
    private Color textColor;
    private const float dissapearTimerMax = 1f;
    Vector3 moveVector;
    private void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
  
        dissapearTimer = dissapearTimerMax;
        moveVector = new Vector3(1, 1) * .3f;
    }

    void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * .015f * Time.deltaTime;
        if (dissapearTimer > dissapearTimerMax * .5f)
        {
            //float decreaseScaleAmount = 1f;
            //transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        dissapearTimer -= Time.deltaTime;


        if (dissapearTimer < 0)
        {
            float dissapearSpeed = 3f;
            textColor.a -= dissapearSpeed * Time.deltaTime;
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
