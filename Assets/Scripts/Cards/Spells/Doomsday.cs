using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doomsday : Spell
{
    protected override void SpecificCastEffect()
    {
        base.SpecificCastEffect();
        foreach (KeyValuePair<int, Creature> kvp in GameManager.singleton.allCreaturesOnField)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Kill();
            }
        }
    }
}
