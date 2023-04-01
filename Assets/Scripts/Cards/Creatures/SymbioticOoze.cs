using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbioticOoze : Creature
{
    public override void OtherCreatureEntered(Creature creatureThatETB)
    {
        base.OtherCreatureEntered(creatureThatETB);
        if (this != null)
        {
            if (creatureThatETB.playerOwningCreature == this.playerOwningCreature && creatureThatETB != this)
            {
                GiveCounter(1);
            }
        }
    }
}
