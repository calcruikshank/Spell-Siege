using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : TargetedSpell
{
    public float timer;
    bool hasStartedCasting;
    protected override void SpecificSpellAbility()
    {
        base.SpecificSpellAbility(); hasStartedCasting = true;

    }

    private void Update()
    {
        if (hasStartedCasting)
        {
            instantiatedObject.transform.position = new Vector3(creatureTargeted.actualPosition.x, .4f, creatureTargeted.actualPosition.z);
            timer += Time.deltaTime;
            if (timer >= 1.7f)
            {
                hasStartedCasting = false;
                creatureTargeted.Kill();
            }
        }
    }
}
