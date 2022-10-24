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
            case CreatureState.Summoned:
                HandleFlyingAnimation();
                break;
            case CreatureState.Idle:
                HandleFlyingAnimation();
                break;
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
        thisTraversableType = travType.Flying;
    }
}
