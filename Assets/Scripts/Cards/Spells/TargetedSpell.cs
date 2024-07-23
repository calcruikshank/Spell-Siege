using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetedSpell : MonoBehaviour
{
    public Controller playerCastingSpell;
    protected Creature creatureTargeted;
    public GameObject instantiatedObject;
    [SerializeField] GameObject GOToInstantiate;

    public bool requiresCreatureBeFriendly = false;
    public void InjectDependencies(Creature creatureTargeted, Controller playerCasting)
    {
        playerCastingSpell = playerCasting;
        this.creatureTargeted = creatureTargeted;
        Cast();
    }

    protected virtual void Cast()
    {
        if (GOToInstantiate != null)
        {
            Debug.Log("instantiating spell object");
            instantiatedObject = Instantiate(GOToInstantiate, new Vector3(creatureTargeted.actualPosition.x, .4f, creatureTargeted.actualPosition.z), Quaternion.identity);
        }
        SpecificSpellAbility();
    }

    protected virtual void SpecificSpellAbility()
    {
        
    }
}
