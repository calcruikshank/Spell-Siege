using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSpawnCreature : MonoBehaviour
{
    float timeElapsed;
    float lerpDuration = .1f;

    [SerializeField] Transform particleHit;
    Vector3 startValue = new Vector3();

    Vector3 endValue = new Vector3();

    Vector3 valueToLerp = new Vector3();

    private void Awake()
    {
        startValue = this.transform.position;
        endValue = new Vector3(this.transform.position.x, .2f, this.transform.position.z);
        Instantiate(particleHit, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
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
        }
    }
}
