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
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3((float)targetedCreature.actualPositionX, (float)targetedCreature.actualPositionY, (float)targetedCreature.actualPositionZ), speed * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, new Vector3((float)targetedCreature.actualPositionX, (float)targetedCreature.actualPositionY, (float)targetedCreature.actualPositionZ)) < .02f && shutDown == false)
            {
                targetedCreature.TakeDamage(amountofdamage);
                if (deathtouch)
                {
                    targetedCreature.Kill();
                }



                TurnOff();
                shutDown = true;
            }
        }
        if (targetedStructure != null)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3( targetedStructure.transform.position.x, .2f, targetedStructure.transform.position.z), 10f * Time.deltaTime); 
            if (Vector3.Distance(this.transform.position, new Vector3(targetedStructure.transform.position.x, .2f, targetedStructure.transform.position.z)) < .02f && shutDown == false)
            {
                targetedStructure.TakeDamage(amountofdamage);

                TurnOff();
                shutDown = true;
            }
        }
        if (targetedStructure == null && targetedCreature == null)
        {

            TurnOff();
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

    public void TurnOff()
    {
        if (this.GetComponentInChildren<ParticleSystem>() != null)
        {
            foreach (ParticleSystem ps in this.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop();
            }
        }
        if (this.GetComponentInChildren<MeshRenderer>() != null)
        {
            foreach (MeshRenderer mr in this.GetComponentsInChildren<MeshRenderer>())
            {
                mr.enabled = false;
            }
        }
        if (this.GetComponentInChildren<Light>() != null)
        {
            foreach (Light l in this.GetComponentsInChildren<Light>())
            {
                l.enabled = false;
            }
        }
    }
}
