using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlumberingAncientSpellCounter : MonoBehaviour
{
    SlumberingAncient sa;
    // Start is called before the first frame update
    void Start()
    {
        sa = this.GetComponent<SlumberingAncient>();
        sa.enabled = false;
        this.GetComponent<Collider>().enabled = false;
    }
    

    private void FixedUpdate()
    {
        if (sa.playerOwningCreature.spellCounter >= 10 && this.enabled)
        {
            this.GetComponent<Collider>().enabled = true;
            sa.enabled = true;
            this.enabled = false;
        }
    }

}
