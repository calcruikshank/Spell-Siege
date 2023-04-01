using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter2Seconds : MonoBehaviour
{
    float timeToDestory = .1f;
    float timer = 0;

    Transform targetTransform;

    Vector3 targetRotation;


    bool fullyreachedback = false;
    void Update()
    {
        this.transform.forward = Vector3.RotateTowards(this.transform.forward, -targetRotation, 20 * Time.deltaTime, 0);
        //this.transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, 10f * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer > timeToDestory)
        {
            Destroy(this.gameObject);
        }
    }

    internal void SetTarget(Transform targetTransformSent)
    {
        targetRotation = new Vector3(targetTransformSent.transform.position.x, transform.position.y, targetTransformSent.transform.position.z) - this.transform.position;
        targetTransform = targetTransformSent;
    }
}
