using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAttackParticle : MonoBehaviour
{
    Creature targetedCreature;
    float amountofdamage;
    public bool shutDown = false;
    public void SetTarget(Creature creatureToTarget, float attack)
    {
        targetedCreature = creatureToTarget;
        amountofdamage = attack;
    }

    private void FixedUpdate()
    {
        if (shutDown)
        {
            return;
        }
        if (targetedCreature != null)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetedCreature.actualPosition, 10f * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, targetedCreature.actualPosition) < .01f && shutDown == false)
            {
                targetedCreature.TakeDamage(amountofdamage);
                this.GetComponent<ParticleSystem>().Stop();
                shutDown = true;
            }
        }
        if (targetedCreature == null)
        {
            this.GetComponent<ParticleSystem>().Stop();
            shutDown = true;
        }
    }
}
