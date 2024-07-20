using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lion : Creature
{
    public override void OnTurn()
    {
        base.OnTurn();

        foreach (KeyValuePair<int, Creature> kvp in playerOwningCreature.creaturesOwned)
        {
            kvp.Value.GiveCounter(1);
        }
    }
}
