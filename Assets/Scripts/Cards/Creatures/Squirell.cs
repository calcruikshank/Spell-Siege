using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squirell : Creature
{
    public override void OnAttack()
    {
        this.GiveCounter(1);
    }
}
