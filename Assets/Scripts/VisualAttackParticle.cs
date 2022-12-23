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
        Vector3 direction = creatureToTarget.transform.position - this.transform.position;
        this.transform.forward = direction;
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
                if (deathtouch)
                {
                    targetedCreature.Kill();
                }

               
                if (this.GetComponentInChildren<ParticleSystem>() != null)
                {
                    foreach (ParticleSystem ps in this.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.Stop();
                    }
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
                    this.GetComponentInChildren<ParticleSystem>().Stop();
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
        Vector3 direction = structureToAttack.transform.position - this.transform.position;
        this.transform.forward = direction;
        amountofdamage = attack;
    }

    bool deathtouch = false;
    internal void SetDeathtouch(Creature creatureToAttack, float attack)
    {
        deathtouch = true;
    }
}
