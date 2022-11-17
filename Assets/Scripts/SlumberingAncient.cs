using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlumberingAncient : Creature
{
    public override void TakeDamage(float attack)
    {
        if (this.enabled == false)
        {
            return;
        }
        base.TakeDamage(attack);
    }
}
