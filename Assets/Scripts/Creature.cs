using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Creature : MonoBehaviour
{
    Transform colorIndicator;
    [SerializeField] public float speed = 1f; //move speed
    [SerializeField] public int range; //num of tiles that can attack
    [SerializeField] float UsageRate = 1f; // the rate at which the minion can use abilities/ attack 


    [SerializeField] public float Attack;
    public float CurrentAttack;
    float AttackRate = 4;
    protected float abilityRate = 4;


    protected float AttackRateTimer;
    protected float abilityRateTimer;
    [HideInInspector] public float CurrentHealth;
    [SerializeField] public float MaxHealth;


    [SerializeField] TextMeshPro attackText;

    bool indestructible = false;
    internal void ToggleIndestructibilty(bool v)
    {
        indestructible = v;
    }

    [SerializeField] TextMeshPro healthText;

    [SerializeField] int numOfTargetables = 1;

    [HideInInspector] public List<BaseTile> allTilesWithinRange;
    [HideInInspector] public int creatureID;
    public CreatureState creatureState;
    [HideInInspector]
    public enum CreatureState
    {
        Summoned, //On The turn created
        Moving,
        Idle,
        Dead
        //not sure if i need a tapped state yet trying to keep it as simple as possible
    }
    SpellSiegeData.CreatureType creatureType;

    bool canAttack = false;
    [HideInInspector] public Controller playerOwningCreature;
    Pathfinding pathfinder1;
    Pathfinding pathfinder2;

    public SpellSiegeData.travType thisTraversableType;


    LineRenderer lr;
    public LineRenderer lr2;
    GameObject lrGameObject;
    GameObject lrGameObject2;

    Tilemap baseTileMap;
    [HideInInspector] public Vector3Int currentCellPosition;

    [HideInInspector] public BaseTile tileCurrentlyOn;
    [HideInInspector] public BaseTile previousTilePosition;

    public Vector3 actualPosition;
    public Vector3 targetedPosition;
    Vector3[] positions;


    List<Vector3> rangePositions = new List<Vector3>();
    protected Grid grid;
    private void Awake()
    {
        this.colorIndicator = transform;
    }

    protected virtual void Start()
    {
        grid = GameManager.singleton.grid;
        baseTileMap = GameManager.singleton.baseMap;
        currentCellPosition = grid.WorldToCell(this.transform.position);
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        previousTilePosition = tileCurrentlyOn;
        tileCurrentlyOn.AddCreatureToTile(this);
        SetupLR();
        SetupLR2();
        SetRangeLineRenderer();
        actualPosition = this.transform.position;

        CalculateAllTilesWithinRange();
        SetTravType();
        pathfinder1 = new Pathfinding();
        pathfinder2 = new Pathfinding();
        UpdateCreatureHUD();
    }
    protected virtual void SetTravType()
    {
    }

    void SetupLR()
    {
        lrGameObject = new GameObject("LineRendererGameObject", typeof(LineRenderer));
        lr = lrGameObject.GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.alignment = LineAlignment.TransformZ;
        lr.transform.localEulerAngles = new Vector3(90, 0, 0);
        lr.sortingOrder = 1000;
        lr.startWidth = .2f;
        lr.endWidth = .2f;
        lr.numCapVertices = 1;
        lr.material = GameManager.singleton.RenderInFrontMat;
        lr.startColor = playerOwningCreature.col;
        lr.endColor = playerOwningCreature.col;
    }
    void SetupLR2()
    {
        lrGameObject2 = new GameObject("LineRendererGameObject2", typeof(LineRenderer));
        lr2 = lrGameObject2.GetComponent<LineRenderer>();
        lr2.enabled = false;
        lr2.alignment = LineAlignment.TransformZ;
        lr2.transform.localEulerAngles = new Vector3(90, 0, 0);
        lr2.sortingOrder = 1000;
        lr2.startWidth = .2f;
        lr2.endWidth = .2f;
        lr2.numCapVertices = 1;
        lr2.material = GameManager.singleton.rangeIndicatorMat;
        lr2.startColor = playerOwningCreature.col;
        lr2.endColor = playerOwningCreature.col;
    }

    protected virtual void Update()
    {
        switch (creatureState)
        {
            case CreatureState.Moving:
                VisualMove();
                break;
        }
    }
    void FixedUpdate()
    {
        switch (creatureState)
        {
            case CreatureState.Moving:
                Move();
                ChooseTarget();
                HandleAttackRate();
                CheckForCreaturesWithinRange();
                HandleAbilityRate();
                DrawLine();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Idle:
                ChooseTarget();
                DrawLine();
                CheckForCreaturesWithinRange();
                HandleAttackRate();
                HandleAbilityRate();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Summoned:
                ChooseTarget();
                DrawLine();
                CheckForCreaturesWithinRange();
                HandleAttackRate();
                HandleAbilityRate();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Dead:
                break;
        }

    }


    internal void IncreaseAttackByX(float v)
    {
        CurrentAttack += v;
        UpdateCreatureHUD();
    }

    public bool IsCreatureWithinRange(Creature creatureSent)
    {
        return allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(creatureSent.currentCellPosition));
    }
    bool IsStructureInRange(Structure structureSent)
    {
        return allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(structureSent.currentCellPosition));
    }


    protected List<Creature> creaturesWithinRange = new List<Creature>();
    protected List<Creature> friendlyCreaturesWithinRange = new List<Creature>();
    protected List<Structure> structresWithinRange = new List<Structure>();

    Creature currentTargetedCreature;
    Structure currentTargetedStructure;
    protected virtual void CheckForCreaturesWithinRange()
    {
        structresWithinRange = new List<Structure>();
        creaturesWithinRange = new List<Creature>();
        friendlyCreaturesWithinRange = new List<Creature>();
        foreach (BaseTile baseTile in allTilesWithinRange)
        {
            if (baseTile.CreatureOnTile() != null)
            {
                if (!creaturesWithinRange.Contains(baseTile.CreatureOnTile()))
                {
                    creaturesWithinRange.Add(baseTile.CreatureOnTile());
                }
                if (baseTile.CreatureOnTile().playerOwningCreature == this.playerOwningCreature)
                {
                    if (baseTile.CreatureOnTile() != this)
                    {
                        if (!friendlyCreaturesWithinRange.Contains(baseTile.CreatureOnTile()))
                        {
                            friendlyCreaturesWithinRange.Add(baseTile.CreatureOnTile());
                        }
                    }
                }
            }
        }
        foreach (BaseTile baseTile in allTilesWithinRange)
        {
            if (baseTile.StructureOnTile() != null)
            {
                if (!structresWithinRange.Contains(baseTile.StructureOnTile()))
                {
                    structresWithinRange.Add(baseTile.StructureOnTile());
                }
            }
        }

    }
    void ChooseTarget()
    {
        float lowestHealthCreatureWithinRange = -1;
        if (currentTargetedCreature != null)
        {
            if (!IsCreatureWithinRange(currentTargetedCreature))
            {
                currentTargetedCreature = null;
            }
        }
        if (targetToFollow != null && targetToFollow.playerOwningCreature != this.playerOwningCreature)
        {
            currentTargetedCreature = targetToFollow;
        }
        /*if (targetToFollow == null || !IsCreatureWithinRange(targetToFollow))
        {
            foreach (Creature creatureInRange in creaturesWithinRange)
            {
                if (creatureInRange.playerOwningCreature != this.playerOwningCreature)
                {
                    if (currentTargetedCreature == null)
                    {
                        lowestHealthCreatureWithinRange = creatureInRange.CurrentHealth;
                        currentTargetedCreature = creatureInRange;
                    }
                    if (currentTargetedCreature != null)
                    {
                        if (creatureInRange.CurrentHealth < lowestHealthCreatureWithinRange)
                        {
                            currentTargetedCreature = creatureInRange;
                            lowestHealthCreatureWithinRange = creatureInRange.CurrentHealth;
                        }
                    }
                }


            }
        }*/



        if (currentTargetedStructure != null)
        {
            if (!IsStructureInRange(currentTargetedStructure))
            {
                currentTargetedStructure = null;
            }
        }
        /*foreach (Structure structureInRange in structresWithinRange)
        {
            if (structureInRange.playerOwningStructure != this.playerOwningCreature)
            {
                if (currentTargetedStructure == null && currentTargetedCreature == null)
                {
                    if (structureInRange.playerOwningStructure != this.playerOwningCreature)
                    {
                        currentTargetedStructure = structureInRange;
                    }
                }
            }
        }*/
        if (structureToFollow != null)
        {
            if (structureToFollow.playerOwningStructure != this.playerOwningCreature)
            {
                currentTargetedStructure = structureToFollow;
                currentTargetedCreature = null;
            }
        }

        #region cleanup
        if (currentTargetedStructure != null && currentTargetedCreature != null)
        {
            if (targetToFollow != null)
            {
                currentTargetedStructure = null;
                return;
            }
            if (structureToFollow != null)
            {
                currentTargetedCreature = null;
                return;
            }
            currentTargetedStructure = null;
        }
        #endregion
    }


    [SerializeField] public Transform visualAttackParticle;


    protected virtual void HandleFriendlyCreaturesList()
    {

    }

    public virtual void HandleAttack()
    {
        CheckForCreaturesWithinRange();
        ChooseTarget();
        if (currentTargetedCreature != null)
        {
            if (IsCreatureWithinRange(currentTargetedCreature))
            {
                VisualAttackAnimation(currentTargetedCreature);
                canAttack = false;
                canAttackIcon.gameObject.SetActive(false);
                OnAttack();
            }
        }
        if (currentTargetedStructure != null)
        {
            if (IsStructureInRange(currentTargetedStructure))
            {
                VisualAttackAnimationOnStructure(currentTargetedStructure);
                canAttack = false;
                canAttackIcon.gameObject.SetActive(false);
                OnAttack();
            }
        }
    }


    protected virtual void VisualAttackAnimation(Creature creatureToAttack)
    {
        if (playerOwningCreature.IsOwner)
        {
            playerOwningCreature.AttackCreatureServerRpc(this.creatureID, creatureToAttack.creatureID);
        }
        if (playerOwningCreature.isAI)
        {
            LocalAttackCreature(creatureToAttack);
        }
    }
    public virtual void LocalAttackCreature(Creature creatureToAttack)
    {
        if (visualAttackParticle != null)
        {
            if (creatureToAttack != null)
            {

                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y + .1f, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.GetComponent<VisualAttackParticle>().SetTarget(creatureToAttack, CurrentAttack);
                if (deathtouch)
                {
                    instantiatedParticle.GetComponent<VisualAttackParticle>().SetDeathtouch(creatureToAttack, CurrentAttack);
                }
                if (range == 1)
                {
                    instantiatedParticle.GetComponent<VisualAttackParticle>().SetRange(1);
                }
                OnAttack();
            }
        }
    }


    protected virtual void VisualAttackAnimationOnStructure(Structure structureToAttack)
    {
        if (playerOwningCreature.IsOwner)
        {
            playerOwningCreature.AttackStructureServerRpc(this.creatureID, structureToAttack.currentCellPosition);
        }
        if (playerOwningCreature.isAI)
        {
            LocalAttackStructure(structureToAttack);
        }
    }
    public void LocalAttackStructure(Structure structureToAttack)
    {
        if (visualAttackParticle != null)
        {
            Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), Quaternion.identity);
            instantiatedParticle.GetComponent<VisualAttackParticle>().SetTargetStructure(structureToAttack, CurrentAttack);
            if (range == 1)
            {
                instantiatedParticle.GetComponent<VisualAttackParticle>().SetRange(1);
            }
            OnAttack();
        }
    }

    public virtual void TakeDamage(float attack)
    {
        GameManager.singleton.SpawnDamageText(new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), attack);
        if (indestructible) return;
        this.CurrentHealth -= attack;
        UpdateCreatureHUD();
        if (this.CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Kill()
    {
        if (indestructible) return;
        Die();
    }
    public void Die()
    {
        if (this.playerOwningCreature.IsOwner)
        {
            playerOwningCreature.DieServerRpc(this.creatureID);
        }

        if (this.playerOwningCreature.isAI)
        {
            playerOwningCreature.LocalDie(this.creatureID);
        }

    }
    public void LocalDie()
    {
        Instantiate(GameManager.singleton.onDeathEffect, new Vector3(actualPosition.x, .4f, actualPosition.z), Quaternion.identity);
        if (this.playerOwningCreature.locallySelectedCreature == this)
        {
            playerOwningCreature.SetStateToNothingSelected();
        }
        lrGameObject.SetActive(false);
        lrGameObject2.SetActive(false);
        rangeLrGO.SetActive(false);
        OnDeath();
        GameManager.singleton.CreatureDied(this.creatureID);
        Destroy(this.gameObject);
    }

    void HandleAttackRate()
    {
        if (canAttack)
        {
            HandleAttack();
        }

    }


    void HandleAbilityRate()
    {
        abilityRateTimer += Time.fixedDeltaTime;
    }
    public void UpdateCreatureHUD()
    {
        this.healthText.text = CurrentHealth.ToString();
        if (CurrentHealth < MaxHealth)
        {
            this.healthText.color = Color.red;
        }
        if (CurrentHealth >= MaxHealth)
        {
            this.healthText.color = Color.white;
        }
        if (CurrentAttack > Attack)
        {
            this.attackText.color = Color.green;
        }
        if (CurrentAttack == Attack)
        {
            this.attackText.color = Color.white;
        }
        if (CurrentAttack < Attack)
        {
            this.attackText.color = Color.red;
        }
        this.attackText.text = CurrentAttack.ToString();
    }

    internal void OnTurn()
    {
        canAttack = true;
        canAttackIcon.gameObject.SetActive(true);
        HandleFriendlyCreaturesList();
        //GiveCounter(1);
    }
    public void GiveCounter(int numOfCounters)
    {
        if (playerOwningCreature.IsOwner)
        {
            playerOwningCreature.GiveCounterServerRpc(this.creatureID, numOfCounters);
        }
        if (playerOwningCreature.isAI)
        {
            LocalGiveCounter(numOfCounters);
        }
    }

    public void LocalGiveCounter(int numOfCounters)
    {
        if (this != null && this.transform != null)
        {
            MaxHealth += numOfCounters;
            CurrentHealth += numOfCounters;
            CurrentAttack += numOfCounters;
            Attack += numOfCounters;


            //for numberofcounters trigger on counter gained
            GameManager.singleton.SpawnLevelUpPrefab(this.transform.position);
            UpdateCreatureHUD();
        }
    }

    [HideInInspector] public List<BaseTile> pathVectorList = new List<BaseTile>();
    int currentPathIndex;



    public virtual void SetMove(Vector3 positionToTarget, Vector3 originalPosition)
    {
        actualPosition = originalPosition;
        HidePathfinderLR();
        rangeLr.enabled = false;
        Vector3Int targetedCellPosition = grid.WorldToCell(new Vector3(positionToTarget.x, 0, positionToTarget.z));

        if (tempLineRendererBetweenCreatures != null)
        {
            tempLineRendererBetweenCreatures.enabled = false;
        }


        //currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
        List<BaseTile> tempPathVectorList = pathfinder1.FindPath(tileCurrentlyOn.tilePosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).tilePosition, thisTraversableType);
        if (tempPathVectorList == null)
        {
            return;
        }
        List<BaseTile> path = tempPathVectorList;
        pathVectorList = path;
        //SetNewTargetPosition(BaseMapTileState.singleton.GetWorldPositionOfCell(path[1].tilePosition));
        //SetNewTargetPosition(positionToTarget);
        SetLRPoints();
        currentPathIndex = 0;
        creatureState = CreatureState.Moving;


    }

    public void SetMoveRpc()
    {
    }

    protected void SetLRPoints()
    {
        List<Vector3> lrList = new List<Vector3>();
        //targetPosition = positionToTarget;

        lrList.Add(actualPosition);
        for (int i = currentPathIndex; i < pathVectorList.Count; i++)
        {
            lrList.Add(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[i].tilePosition));
        }
        positions = lrList.ToArray();
        //positions[0] = this.transform.position;
        //positions[1] = targetPosition;
        lr.enabled = true;
        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
    }

    public void Move()
    {
        if (pathVectorList != null)
        {
            CheckForCreaturesInPath();
            if (currentPathIndex < pathVectorList.Count)
            {
                targetedPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[currentPathIndex].tilePosition);
            }

            if (Vector3.Distance(actualPosition, targetedPosition) > .02f)
            {
                actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedPosition.x, actualPosition.y, targetedPosition.z), speed * Time.fixedDeltaTime);
                SetLRPoints();
            }

            if (Vector3.Distance(actualPosition, targetedPosition) <= .02f)
            {
                if (currentPathIndex >= pathVectorList.Count - 1)
                {
                    SetStateToIdle();
                }
                else
                {
                    currentPathIndex++;
                }
            }
            if (tileCurrentlyOn != null)
            {
                if (pathVectorList.Count > 0)
                {
                    if (tileCurrentlyOn == pathVectorList[0] && currentPathIndex == 0 && pathVectorList.Count > 1)
                    {
                        if (pathVectorList.Count > 1)
                        {
                            currentPathIndex++; //for keeping position
                        }
                    }
                }
            }
        }

        currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
        if (BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition) == null)
        {
            tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        }
        if (BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition) != null && tileCurrentlyOn.tilePosition != currentCellPosition)
        {
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[pathVectorList.Count - 1].tilePosition), actualPosition);
        }
        if (previousTilePosition != tileCurrentlyOn)
        {
            CalculateAllTilesWithinRange();
            previousTilePosition.RemoveCreatureFromTile(this);
            previousTilePosition = tileCurrentlyOn;
            tileCurrentlyOn.AddCreatureToTile(this);
        }



    }

    void DrawLine()
    {
        Transform targetToDrawLineTo;
        if (!currentTargetedCreature && !currentTargetedStructure)
        {
            if (tempLineRendererBetweenCreaturesGameObject != null)
            {
                tempLineRendererBetweenCreaturesGameObject.SetActive(false);
                tempLineRendererBetweenCreatures.enabled = false;
            }
        }
        if (currentTargetedCreature != null)
        {
            targetToDrawLineTo = currentTargetedCreature.transform;
            DrawLineToTargetedCreature(targetToDrawLineTo.transform.position);
        }
        if (currentTargetedStructure != null)
        {
            DrawLineToTargetedCreature(BaseMapTileState.singleton.GetWorldPositionOfCell(currentTargetedStructure.tileCurrentlyOn.tilePosition));

        }
    }
    BaseTile lastTileFollowCreatureWasOn;


    LineRenderer tempLineRendererBetweenCreatures;

    public GameObject tempLineRendererBetweenCreaturesGameObject;
    private void DrawLineToTargetedCreature(Vector3 positionSent)
    {
        if (tempLineRendererBetweenCreaturesGameObject == null)
        {
            GenerateTempLineRendererBetweenThisAndTarget();
        }
        if (tempLineRendererBetweenCreaturesGameObject != null)
        {
            tempLineRendererBetweenCreaturesGameObject.SetActive(true);
            tempLineRendererBetweenCreatures.enabled = true;
            List<Vector3> tempPositions = new List<Vector3>();
            tempPositions.Add(new Vector3(this.actualPosition.x, this.actualPosition.y + .2f, this.actualPosition.z));
            tempPositions.Add(new Vector3(positionSent.x, positionSent.y + .2f, positionSent.z));
            tempLineRendererBetweenCreatures.SetPositions(tempPositions.ToArray());
        }
    }

    void GenerateTempLineRendererBetweenThisAndTarget()
    {
        tempLineRendererBetweenCreaturesGameObject = new GameObject("LineRendererGameObjectNBetweenCreatures", typeof(LineRenderer));
        tempLineRendererBetweenCreatures = tempLineRendererBetweenCreaturesGameObject.GetComponent<LineRenderer>();
        tempLineRendererBetweenCreatures.enabled = false;
        tempLineRendererBetweenCreatures.alignment = LineAlignment.TransformZ;
        tempLineRendererBetweenCreatures.transform.localEulerAngles = new Vector3(90, 0, 0);
        tempLineRendererBetweenCreatures.sortingOrder = 1000;
        tempLineRendererBetweenCreatures.startWidth = .05f;
        tempLineRendererBetweenCreatures.endWidth = .05f;
        tempLineRendererBetweenCreatures.numCapVertices = 1;
        tempLineRendererBetweenCreatures.material = GameManager.singleton.RenderInFrontMat;
        tempLineRendererBetweenCreatures.startColor = playerOwningCreature.col;
        tempLineRendererBetweenCreatures.endColor = playerOwningCreature.col;
    }


    //its not recursive i swear
    private void CheckForCreaturesInPath()
    {
        if (currentPathIndex < pathVectorList.Count - 1)
        {
            if (BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex + 1].tilePosition) != null)
            {
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[pathVectorList.Count - 1].tilePosition), actualPosition);
            }
        }
    }

    protected void VisualMove()
    {
        this.transform.position = actualPosition;
        return;
        float valueToAdd = 0f;
        positions[0] = this.transform.position;
        lr.SetPositions(positions);
        lr.startColor = playerOwningCreature.col;
        lr.endColor = playerOwningCreature.col;
        float distanceFromActualPosition = (this.transform.position - actualPosition).magnitude;
        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(targetedPosition.x, actualPosition.y, targetedPosition.z), speed * Time.deltaTime);
    }

    public Transform canAttackIcon;
    internal void SetToPlayerOwningCreature(Controller controller)
    {
        creatureState = CreatureState.Summoned;
        creatureID = GameManager.singleton.allCreatureGuidCounter;
        GameManager.singleton.allCreaturesOnField.Add(creatureID, this);
        GameManager.singleton.allCreatureGuidCounter++;
        this.playerOwningCreature = controller;
        this.transform.GetComponent<MeshRenderer>().material.color = controller.col;
        //colorIndicator.GetComponent<SpriteRenderer>().color = controller.col;
        OnETB();
        canAttack = true;
        canAttackIcon = Instantiate(GameManager.singleton.canAttackIcon);
        canAttackIcon.parent = transform;
        canAttackIcon.localScale = new Vector3(.4f, .4f, .4f);
        canAttackIcon.localPosition = new Vector3(0, 1.6f, 0);
        canAttackIcon.localEulerAngles = new Vector3(90, 90, 130);
    }

    public void SetStateToIdle()
    {
        if (playerOwningCreature.IsOwner)
        {
            playerOwningCreature.SetCreatureToIdleServerRpc(creatureID, currentCellPosition);
        }
        tileCurrentlyOn.RemoveCreatureFromTile(this);
        lr.enabled = false;

        lrGameObject.SetActive(true);
        lrGameObject2.SetActive(true);
        HidePathfinderLR();
        this.actualPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(currentCellPosition);
        this.transform.position = actualPosition;
        currentCellPosition = grid.WorldToCell(new Vector3(this.actualPosition.x, 0, this.actualPosition.z));
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        tileCurrentlyOn.AddCreatureToTile(this);
        creatureState = CreatureState.Idle;
    }

    #region range


    internal void LocalSetCreatureToIdle(Vector3Int actualPositionSent)
    {
        if (!playerOwningCreature.IsOwner)
        {
            tileCurrentlyOn.RemoveCreatureFromTile(this);
            lr.enabled = false;

            lrGameObject.SetActive(true);
            lrGameObject2.SetActive(true);
            HidePathfinderLR();
            this.actualPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(actualPositionSent);
            this.transform.position = actualPosition;
            currentCellPosition = grid.WorldToCell(new Vector3(this.actualPosition.x, 0, this.actualPosition.z));
            tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            tileCurrentlyOn.AddCreatureToTile(this);
            creatureState = CreatureState.Idle;
        }

    }

    internal void AddOneRange()
    {
        this.range++;
        CalculateAllTilesWithinRange();
    }
    internal void SubtractOneRange()
    {
        this.range--;
        CalculateAllTilesWithinRange();
    }
    void CalculateAllTilesWithinRange()
    {
        List<Vector3Int> extents = new List<Vector3Int>();
        allTilesWithinRange.Clear();
        rangePositions.Clear();
        int xthreshold;
        int threshold;
        for (int x = 0; x < range + 1; x++)
        {
            for (int y = 0; y < range + 1; y++)
            {
                xthreshold = range - x;
                threshold = range + xthreshold;

                if (y + x > threshold)
                {

                    if (currentCellPosition.y % 2 == 0)
                    {
                        if (y + x <= threshold + 1)
                        {
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z)));
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z)));
                        }
                    }
                    if (currentCellPosition.y % 2 != 0)
                    {
                        if (y + x <= threshold + 1)
                        {
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z)));
                            allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z)));
                        }
                    }
                    continue;
                }
                if (y == range && currentCellPosition.y % 2 == 0)
                {
                    if (range % 2 != 0 && y + x == threshold - 1)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                    if (range % 2 == 0 && y + x == threshold)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                }
                if (y == range && currentCellPosition.y % 2 != 0)
                {
                    if (range % 2 != 0 && y + x == threshold - 1)
                    {
                        extents.Add(new Vector3Int(x + 1, y, currentCellPosition.z));
                    }
                    if (range % 2 == 0 && y + x == threshold)
                    {
                        extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                    }
                }
                if (x == range && y + x == threshold)
                {
                    extents.Add(new Vector3Int(x, y, currentCellPosition.z));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y + y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + x, currentCellPosition.y - y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y + y, currentCellPosition.z)));
                }
                if (!allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z))))
                {
                    allTilesWithinRange.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - x, currentCellPosition.y - y, currentCellPosition.z)));
                }

            }
        }

        extents.Add(new Vector3Int(extents[0].x, -extents[0].y, extents[0].z));
        if (range % 2 != 0)
        {
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, -extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x - 1, -extents[0].y, extents[0].z));
            }
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, +extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x - 1, +extents[0].y, extents[0].z));
            }
        }
        if (range % 2 == 0)
        {
            extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            extents.Add(new Vector3Int(-extents[0].x, +extents[0].y, extents[0].z));
            /*if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, -extents[0].y, extents[0].z));
            }
            extents.Add(new Vector3Int(-extents[1].x, extents[1].y, extents[1].z));
            if (currentCellPosition.y % 2 != 0)
            {
                extents.Add(new Vector3Int(-extents[0].x + 1, +extents[0].y, extents[0].z));
            }
            if (currentCellPosition.y % 2 == 0)
            {
                extents.Add(new Vector3Int(-extents[0].x, +extents[0].y, extents[0].z));
            }*/
        }
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).topRight);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[1]).topRight);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[1]).bottomRight);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[2]).bottomRight);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[2]).bottom);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[3]).bottom);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[3]).bottomLeft);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[4]).bottomLeft);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[4]).topLeft);

        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[5]).topLeft);
        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[5]).top);

        rangePositions.Add(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition + extents[0]).top);


        List<Vector3> newRangePositions = new List<Vector3>();


        SetNewPositionsForRangeLr(rangePositions);
    }

    public bool lifelink = false;
    public bool deathtouch = false;
    public bool taunt = false;

    GameObject rangeLrGO;
    LineRenderer rangeLr;
    private void SetRangeLineRenderer()
    {
        rangeLrGO = new GameObject("LineRendererGameObjectForRange", typeof(LineRenderer));
        rangeLr = rangeLrGO.GetComponent<LineRenderer>();
        rangeLr.enabled = false;
        rangeLr.alignment = LineAlignment.TransformZ;
        rangeLr.transform.localEulerAngles = new Vector3(90, 0, 0);
        rangeLr.sortingOrder = 1000;
        rangeLr.startWidth = .2f;
        rangeLr.endWidth = .2f;
        rangeLr.numCapVertices = 1;
        rangeLr.material = GameManager.singleton.rangeIndicatorMat;
        rangeLr.startColor = playerOwningCreature.col;
        rangeLr.endColor = playerOwningCreature.col;
    }


    CancellationTokenSource s_cts;
    internal void ShowPathfinderLinerRendererAsync(Vector3Int hoveredTilePosition)
    {
        s_cts = new CancellationTokenSource();
        List<BaseTile> tempPathVectorList = pathfinder2.FindPath(currentCellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(hoveredTilePosition).tilePosition, thisTraversableType);
        List<Vector3> lrList = new List<Vector3>();
        //targetPosition = positionToTarget;
        if (tempPathVectorList == null)
        {
            lr2.enabled = false;
            return;
        }
        if (tempPathVectorList != null)
        {
            for (int i = 0; i < tempPathVectorList.Count; i++)
            {
                lrList.Add(BaseMapTileState.singleton.GetWorldPositionOfCell(tempPathVectorList[i].tilePosition));
            }
        }

        lr2.enabled = true;
        lr2.positionCount = lrList.Count;
        lr2.SetPositions(lrList.ToArray());
    }
    public async Task<List<BaseTile>> FindPathAsync(Vector3Int hoveredTilePosition, CancellationTokenSource cts)
    {
        return await Task.FromResult(pathfinder2.FindPath(currentCellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(hoveredTilePosition).tilePosition, thisTraversableType));
    }
    internal void HidePathfinderLR()
    {
        if (positions == null)
        {
            positions = new Vector3[2];
        }
        lr2.enabled = false;
        lr2.positionCount = positions.Length;
        lr2.SetPositions(positions);
    }
    void SetNewPositionsForRangeLr(List<Vector3> rangePositionsSent)
    {
        //rangeLr.enabled = true;

        rangeLr.positionCount = rangePositionsSent.Count;
        rangeLr.SetPositions(rangePositionsSent.ToArray());
    }

    CardInHand originalCard;
    Transform originalCardTransform;
    internal void SetOriginalCard(CardInHand cardSelected)
    {
        this.Attack = cardSelected.currentAttack;
        this.MaxHealth = cardSelected.currentAttack;
        this.CurrentAttack = cardSelected.currentAttack;
        this.CurrentHealth = cardSelected.currentHealth;
        this.creatureType = cardSelected.creatureType;
        Debug.Log("Setting original card to " + cardSelected);
        originalCard = cardSelected;
        originalCardTransform = Instantiate(cardSelected.transform, GameManager.singleton.scalableUICanvas.transform);
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        originalCardTransform.transform.localEulerAngles = Vector3.zero;
        originalCardTransform.transform.localScale = originalCardTransform.transform.localScale * 1f;

        originalCardTransform.GetComponentInChildren<BoxCollider>().enabled = false;
        originalCardTransform.gameObject.SetActive(false);
        UpdateCreatureHUD();

    }

    private void OnMouseOver()
    {
        if (playerOwningCreature.ShowingPurchasableHarvestTiles)
        {
            if (playerOwningCreature.tilesOwned.ContainsValue(tileCurrentlyOn))
            {
                if (!playerOwningCreature.harvestedTiles.Contains(tileCurrentlyOn))
                {
                    OnMouseExit();
                    return;
                }
            }
        }
        rangeLr.enabled = true;
        if (playerOwningCreature.locallySelectedCard != null)
        {
            if (playerOwningCreature.locallySelectedCard.cardType != SpellSiegeData.CardType.Spell)
            {
                playerOwningCreature.locallySelectedCard.gameObject.SetActive(false);
            }
        }
        if (playerOwningCreature.ShowingPurchasableHarvestTiles && playerOwningCreature.tilesOwned.ContainsValue(tileCurrentlyOn) && !tileCurrentlyOn.isBeingHarvested)
        {
            originalCardTransform.gameObject.SetActive(false);
            return;
        }
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);

        originalCardTransform.transform.localScale = Vector3.one * 100 / originalCardTransform.transform.position.z;
        originalCardTransform.gameObject.SetActive(true);

    }

    private void OnMouseExit()
    {
        //if (playerOwningCreature.locallySelectedCreature != this)
        //{
        if (originalCardTransform != null)
        {
            originalCardTransform.gameObject.SetActive(false);
        }
        if (rangeLr != null)
        {
            rangeLr.enabled = false;
        }
        if (playerOwningCreature.locallySelectedCard != null)
        {
            playerOwningCreature.locallySelectedCard.gameObject.SetActive(true);
        }

        //}
    }


    #endregion

    #region Overridables
    public virtual void Garrison() { }
    public virtual void OnETB()
    {
        GameManager.singleton.CreatureEntered(creatureID);
    }

    private void OnDestroy()
    {
        OnMouseExit();
    }
    public virtual void OnDeath()
    {
        SetStateToDead();
    }
    public virtual void OnDamaged() { }
    public virtual void OnHealed() { }
    public virtual void OtherCreatureDied(Creature creatureThatDied)
    {
        if (creatureThatDied == targetToFollow)
        {
            targetToFollow = null;
        }
    }
    public virtual void OtherCreatureEntered(Creature creature)
    {
    }
    public virtual void OnOwnerCastSpell()
    {
    }
    public virtual void Taunt(Creature creatureTaunting)
    {
    }
    public virtual void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        GameManager.singleton.SpawnHealText(this.transform.position, amount);
        UpdateCreatureHUD();
    }

    public virtual void OnAttack()
    {

    }

    void SetStateToDead()
    {
        if (targetToFollow != null)
        {
            targetToFollow = null;
        }
        if (structureToFollow != null)
        {
            structureToFollow = null;
        }
        if (tempLineRendererBetweenCreaturesGameObject != null)
        {
            tempLineRendererBetweenCreaturesGameObject.SetActive(false);
        }
        if (pathVectorList != null)
        {
            pathVectorList = null;
        }
        if (this.playerOwningCreature.selectedCreaturesWithBox.Contains(this))
        {
            this.playerOwningCreature.selectedCreaturesWithBox.Remove(this);
        }
        this.playerOwningCreature.creaturesOwned.Remove(this.creatureID);
        OnMouseExit();
        creatureState = CreatureState.Dead;
    }
    public void SetStateToExiled()
    {
        if (targetToFollow != null)
        {
            targetToFollow = null;
        }
        if (structureToFollow != null)
        {
            structureToFollow = null;
        }
        if (tempLineRendererBetweenCreaturesGameObject != null)
        {
            tempLineRendererBetweenCreaturesGameObject.SetActive(false);
        }
        if (pathVectorList != null)
        {
            pathVectorList = null;
        }
        if (this.playerOwningCreature.selectedCreaturesWithBox.Contains(this))
        {
            this.playerOwningCreature.selectedCreaturesWithBox.Remove(this);
        }
        lrGameObject.SetActive(false);
        lrGameObject2.SetActive(false);

        tileCurrentlyOn.RemoveCreatureFromTile(this);
        OnMouseExit();
        creatureState = CreatureState.Dead;
    }

    public Creature targetToFollow;
    internal void SetTargetToFollow(Creature creatureToFollow, Vector3 originalCreaturePosition)
    {
        if (structureToFollow != null)
        {
            structureToFollow = null;
        }
        if (creatureToFollow != this)
        {
            if (creatureToFollow.playerOwningCreature != this.playerOwningCreature)
            {
                targetToFollow = creatureToFollow;
                if (targetToFollow != null)
                {
                    if (IsCreatureWithinRange(targetToFollow))
                    {
                        return;
                    }
                }
            }
            if (!IsCreatureWithinRange(creatureToFollow))
            {
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(creatureToFollow.tileCurrentlyOn.tilePosition), originalCreaturePosition);
            }
            if (creatureToFollow.playerOwningCreature == this.playerOwningCreature)
            {
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(creatureToFollow.tileCurrentlyOn.tilePosition), originalCreaturePosition);
            }
        }
    }
    public Structure structureToFollow;
    internal void SetStructureToFollow(Structure structureToFollowSent, Vector3 originalCreaturePosition)
    {
        if (targetToFollow != null)
        {
            targetToFollow = null;
        }
        if (structureToFollowSent.playerOwningStructure != this.playerOwningCreature || playerOwningCreature.isAI)
        {
            structureToFollow = structureToFollowSent;
            if (IsStructureInRange(structureToFollowSent))
            {
                return;
            }
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(structureToFollowSent.tileCurrentlyOn.tilePosition), originalCreaturePosition);
        }
    }



    #endregion

}
