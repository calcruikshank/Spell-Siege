using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hamster : Creature
{
    public override void OnDeath()
    {
        for (int i = 0; i < this.currentAttack; i++)
        {
            playerOwningCreature.DrawCard();
        }
        base.OnDeath();
    }
}
