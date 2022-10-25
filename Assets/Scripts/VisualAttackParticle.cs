using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAttackParticle : MonoBehaviour
{
    Creature targetedCreature;
    float amountofdamage;
    public bool shutDown = false;
    Structure targetedStructure;
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
        if (targetedStructure != null)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetedStructure.transform.position, 10f * Time.deltaTime); 
            if (Vector3.Distance(this.transform.position, targetedStructure.transform.position) < .01f && shutDown == false)
            {
                targetedStructure.TakeDamage(amountofdamage);
                this.GetComponent<ParticleSystem>().Stop();
                shutDown = true;
            }
        }
        if (targetedStructure == null && targetedCreature == null)
        {
            this.GetComponentInChildren<ParticleSystem>().Stop();
            shutDown = true;
        }
    }

    internal void SetTargetStructure(Structure structureToAttack, float attack)
    {
        targetedStructure = structureToAttack;
        amountofdamage = attack;
    }
}
