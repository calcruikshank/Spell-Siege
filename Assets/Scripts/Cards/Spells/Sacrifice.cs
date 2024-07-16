using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sacrifice : TargetedSpell
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
            if (timer >= .4f)
            {
                hasStartedCasting = false;
                for (int i = 0; i < 3; i++)
                {
                    creatureTargeted.playerOwningCreature.AddSpecificManaToPool(SpellSiegeData.ManaType.Black);
                }
                creatureTargeted.Kill();
            }
        }
    }
}
