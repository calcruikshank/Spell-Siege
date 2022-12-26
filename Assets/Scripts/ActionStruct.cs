using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ActionStruct
{
    public Controller.ActionTaken actionType;
    public Vector3Int actionInputVector;
    public Vector3 positionOfCreature;
    public bool actionInputBool;
    public int actionInputInt;
    public ActionStruct(Controller.ActionTaken at, Vector3Int inputEnacted, Vector3 positionOfCreatureSent)
    {
        this.actionType = at;
        this.actionInputVector = inputEnacted;
        this.actionInputInt = 0;
        this.actionInputBool = false;
        this.positionOfCreature = positionOfCreatureSent;
    }
    public ActionStruct(Controller.ActionTaken at, bool inputEnacted)
    {
        this.actionType = at;
        this.actionInputVector = Vector3Int.zero;
        this.actionInputInt = 0;
        this.actionInputBool = inputEnacted;
        this.positionOfCreature = Vector3.zero;
    }
    public ActionStruct(Controller.ActionTaken at, int inputEnacted)
    {
        this.actionType = at;
        this.actionInputVector = Vector3Int.zero;
        this.actionInputInt = inputEnacted;
        this.actionInputBool = false;
        this.positionOfCreature = Vector3.zero;
    }
}
