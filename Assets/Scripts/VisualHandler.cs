using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualHandler : MonoBehaviour
{
    Controller player;
    public Creature locallySelectedCreature;

    public void InnjectPlayer(Controller playerSent)
    {
        this.player = playerSent;
    }
}
