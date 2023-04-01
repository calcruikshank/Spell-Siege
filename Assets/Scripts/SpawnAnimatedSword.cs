using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnimatedSword : MonoBehaviour
{
    [SerializeField] Transform swordToSpawn;

    public void SpawnSword(Transform targetTransform)
    {
        Transform InstantiatedSword = Instantiate(swordToSpawn, this.transform.position, this.transform.rotation);
        InstantiatedSword.GetComponent<DestroyAfter2Seconds>().SetTarget(targetTransform);

    }
}
