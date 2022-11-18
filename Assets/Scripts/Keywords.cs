using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keywords : MonoBehaviour
{
    [SerializeField] public Transform lifelinkIndicator;
    [SerializeField] public Transform deathtouchIndicator;
    [SerializeField] public Transform flyingIndicator;
    [SerializeField] public Transform amphibiousIndicator;
    [SerializeField] public Transform tauntIndicator;

    public void InjectDependencies(Card cardSent)
    {
        lifelinkIndicator.gameObject.SetActive(cardSent.lifelink);
        deathtouchIndicator.gameObject.SetActive(cardSent.deathtouch);
        flyingIndicator.gameObject.SetActive(cardSent.flying);
        amphibiousIndicator.gameObject.SetActive(cardSent.amphibious);
        tauntIndicator.gameObject.SetActive(cardSent.taunt);
    }
}
