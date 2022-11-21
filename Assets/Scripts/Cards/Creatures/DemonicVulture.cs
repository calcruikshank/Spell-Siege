using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicVulture : Creature
{
    public override void OtherCreatureDied(Creature creatureThatDied)
    {
        base.OtherCreatureDied(creatureThatDied);
        GiveCounter(1);
    }
}
