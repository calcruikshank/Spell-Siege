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

    [SerializeField] float speed = 10f;
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
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetedCreature.actualPosition, speed * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, targetedCreature.actualPosition) < .02f && shutDown == false)
            {
                targetedCreature.TakeDamage(amountofdamage);

                if (this.GetComponentInChildren<ParticleSystem>() != null)
                {
                    this.GetComponent<ParticleSystem>().Stop();
                }
                shutDown = true;
            }
        }
        if (targetedStructure != null)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3( targetedStructure.transform.position.x, .2f, targetedStructure.transform.position.z), 10f * Time.deltaTime); 
            if (Vector3.Distance(this.transform.position, new Vector3(targetedStructure.transform.position.x, .2f, targetedStructure.transform.position.z)) < .02f && shutDown == false)
            {
                targetedStructure.TakeDamage(amountofdamage);
                if (this.GetComponentInChildren<ParticleSystem>() != null)
                {
                    this.GetComponent<ParticleSystem>().Stop();
                }

                shutDown = true;
            }
        }
        if (targetedStructure == null && targetedCreature == null)
        {
            if (this.GetComponentInChildren<ParticleSystem>() != null) 
            {
                this.GetComponentInChildren<ParticleSystem>().Stop();
            }
            
            shutDown = true;
        }
    }

    internal void SetTargetStructure(Structure structureToAttack, float attack)
    {
        targetedStructure = structureToAttack;
        amountofdamage = attack;
    }
}
