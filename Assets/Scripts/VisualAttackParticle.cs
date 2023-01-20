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
        Vector3 direction = new Vector3( creatureToTarget.transform.position.x, this.transform.position.y, creatureToTarget.transform.position.z) - this.transform.position;
        this.transform.forward = direction;
        amountofdamage = attack;
    }


    float timer = 0;
    float timerThreshold = .1f;
    private void FixedUpdate()
    {
        if (range != 1)
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



                    TurnOff();
                    shutDown = true;
                }
            }
            if (targetedStructure != null)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(targetedStructure.transform.position.x, .2f, targetedStructure.transform.position.z), 10f * Time.deltaTime);
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
        else
        {
            if (shutDown)
            {
                return;
            }
            if (targetedCreature != null)
            {
                //this.transform.position = Vector3.MoveTowards(this.transform.position, targetedCreature.actualPosition, speed * Time.deltaTime);
                timer += Time.fixedDeltaTime;
                if (timer > timerThreshold)
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
                timer += Time.fixedDeltaTime;
                if (timer > timerThreshold)
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
    }

    internal void SetTargetStructure(Structure structureToAttack, float attack)
    {
        targetedStructure = structureToAttack;
        Vector3 direction = new Vector3(structureToAttack.transform.position.x, this.transform.position.y, structureToAttack.transform.position.z) - this.transform.position;
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
    int range = 0;
    internal void SetRange(int v)
    {
        range = v;
    }
}
