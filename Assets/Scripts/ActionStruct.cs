using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ActionStruct
{
    public Controller.ActionTaken actionType;
    public Vector3Int actionInputVector;
    public bool actionInputBool;
    public int actionInputInt;
    public ActionStruct(Controller.ActionTaken at, Vector3Int inputEnacted)
    {
        this.actionType = at;
        this.actionInputVector = inputEnacted;
        this.actionInputInt = 0;
        this.actionInputBool = false;
    }
    public ActionStruct(Controller.ActionTaken at, bool inputEnacted)
    {
        this.actionType = at;
        this.actionInputVector = Vector3Int.zero;
        this.actionInputInt = 0;
        this.actionInputBool = inputEnacted;
    }
    public ActionStruct(Controller.ActionTaken at, int inputEnacted)
    {
        this.actionType = at;
        this.actionInputVector = Vector3Int.zero;
        this.actionInputInt = inputEnacted;
        this.actionInputBool = false;
    }
}
