using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetedSpell : MonoBehaviour
{
    Controller playerCastingSpell;
    protected Creature creatureTargeted;
    GameObject instantiatedObject;
    [SerializeField] GameObject GOToInstantiate;
    public void InjectDependencies(Creature creatureTargeted, Controller playerCasting)
    {
        playerCastingSpell = playerCasting;
        this.creatureTargeted = creatureTargeted;
        Cast();
    }

    private void Cast()
    {
        if (instantiatedObject != null)
        {
            instantiatedObject = Instantiate(GOToInstantiate, new Vector3((float)creatureTargeted.actualPositionX, (float)creatureTargeted.actualPositionY, (float)creatureTargeted.actualPositionZ), Quaternion.identity);
        }
        SpecificSpellAbility();
    }

    protected virtual void SpecificSpellAbility()
    {
        
    }
}
