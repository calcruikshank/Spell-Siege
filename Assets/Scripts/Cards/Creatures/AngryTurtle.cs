using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryTurtle : Creature
{
    List<Creature> tauntedCreatures = new List<Creature>();
    public void TauntCreatures()
    {
        foreach (Creature tauntedCreature in tauntedCreatures)
        {
            if (tauntedCreature.playerOwningCreature != this.playerOwningCreature)
            {
                tauntedCreature.Taunt(this);
            }
        }
    }

    protected override void CheckForCreaturesWithinRange()
    {
        base.CheckForCreaturesWithinRange();
        tauntedCreatures = creaturesWithinRange;
        TauntCreatures();
    }
}
