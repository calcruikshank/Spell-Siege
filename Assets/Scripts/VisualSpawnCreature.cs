using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSpawnCreature : MonoBehaviour
{
    float timeElapsed;
    float lerpDuration = .6f;
    Vector3 startValue = new Vector3();

    Vector3 endValue = new Vector3();

    Vector3 valueToLerp = new Vector3();

    private void Awake()
    {
        startValue = this.transform.position;
        endValue = new Vector3(this.transform.position.x, .2f, this.transform.position.z);
    }
    void Update()
    {
        if (timeElapsed < lerpDuration)
        {
            valueToLerp = Vector3.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;

            this.transform.position = valueToLerp;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
