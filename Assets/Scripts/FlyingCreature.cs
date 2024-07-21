using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCreature : Creature
{
    float animationFlyingSpeed = 1f;
    float animationFlyingHeight = .2f;
    float lifeTime = 0;

    protected override void Update()
    {
        switch (creatureState)
        {
            case CreatureState.Moving:
                VisualMove();
                HandleFlyingAnimation();
                break;
            case CreatureState.Idle:
                transform.position = new Vector3(transform.position.x, .1f, transform.position.z);
                break;
        }
        if (targetToFollow != null)
        {
            Vector3 targetRotation = new Vector3(targetToFollow.transform.position.x, transform.position.y, targetToFollow.transform.position.z) - this.transform.position;
            creatureImage.forward = Vector3.RotateTowards(creatureImage.forward, targetRotation, 10 * Time.deltaTime, 0);
        }
        if (structureToFollow != null)
        {
            Vector3 targetRotation = new Vector3(structureToFollow.transform.position.x, transform.position.y, structureToFollow.transform.position.z) - this.transform.position;
            creatureImage.forward = Vector3.RotateTowards(creatureImage.forward, targetRotation, 10 * Time.deltaTime, 0);
        }

        if (canAttackIcon != null)
        {
            canAttackIcon.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + .1f, this.transform.position.z);
            canAttackIcon.transform.localEulerAngles = new Vector3(creatureImage.localEulerAngles.x, creatureImage.localEulerAngles.y + 45, creatureImage.localEulerAngles.z);
        }
    }

    void HandleFlyingAnimation()
    {
        lifeTime += Time.deltaTime;
        //get the objects current position and put it in a variable so we can access it later with less code
        //calculate what the new Y position will be
        float newY = Mathf.Sin(lifeTime * animationFlyingSpeed) * animationFlyingHeight;
        //set the object's Y to the new calculated Y
        transform.position = new Vector3(transform.position.x,(.5f + newY ), transform.position.z);
    }

    protected override void SetTravType()
    {
        thisTraversableType = SpellSiegeData.travType.Flying;
    }
}
