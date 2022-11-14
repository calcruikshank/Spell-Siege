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
    [SerializeField] Transform colorIndicator;
    [SerializeField] float speed = 1f; //move speed
    [SerializeField] int range; //num of tiles that can attack
    [SerializeField] float UsageRate = 1f; // the rate at which the minion can use abilities/ attack 
    [SerializeField] protected float Attack;
    float AttackRate = 4;
    protected float abilityRate = 4;
    protected float AttackRateTimer;
    protected float abilityRateTimer;
    [HideInInspector] public float CurrentHealth;
    [SerializeField] public float MaxHealth;

    [SerializeField] TextMeshPro attackText;
    [SerializeField] TextMeshPro healthText;

    [SerializeField] int numOfTargetables = 1;

    [HideInInspector] public List<BaseTile> allTilesWithinRange;
    [HideInInspector] public int creatureID;
    [HideInInspector] public int ownedCreatureID;
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


    bool canAttack = false;
    [HideInInspector] public Controller playerOwningCreature;
    Pathfinding pathfinder1;
    Pathfinding pathfinder2;

    public enum travType
    {
        Walking,
        Swimming,
        SwimmingAndWalking,
        Flying
    }
    public travType thisTraversableType;


    LineRenderer lr;
    LineRenderer lr2;
    GameObject lrGameObject;
    GameObject lrGameObject2;

    Tilemap baseTileMap;
    [HideInInspector] public Vector3Int currentCellPosition;

    [HideInInspector] public BaseTile tileCurrentlyOn;
    [HideInInspector] public BaseTile previousTilePosition;

    public Vector3 actualPosition;
    Vector3 targetedPosition;
    Vector3[] positions;

    List<Vector3> rangePositions = new List<Vector3>();
    protected Grid grid;
    private void Awake()
    {
        creatureState = CreatureState.Summoned;
    }

    protected virtual void Start()
    {
        GameManager.singleton.tick += OnTick;
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
        creatureID = GameManager.singleton.allCreatureGuidCounter;
        GameManager.singleton.allCreaturesOnField.Add(creatureID, this);
        GameManager.singleton.allCreatureGuidCounter++;
        CurrentHealth = MaxHealth;
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
                CheckForCreaturesWithinRange();
                ChooseTarget();
                HandleAttackRate();
                HandleAbilityRate(); DrawLine();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Idle:
                CheckForCreaturesWithinRange();
                ChooseTarget(); DrawLine();
                HandleAttackRate();
                HandleAbilityRate();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                CheckForFollowTarget();
                break;
            case CreatureState.Summoned:
                CheckForCreaturesWithinRange();
                ChooseTarget();
                DrawLine();
                HandleAttackRate();
                HandleAbilityRate();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                CheckForFollowTarget();
                break;
            case CreatureState.Dead:
                break;
        }
    }

    bool IsCreatureWithinRange(Creature creatureSent)
    {
        return allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(creatureSent.currentCellPosition));
    }
    bool IsStructureInRange(Structure structureSent)
    {
        return allTilesWithinRange.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(structureSent.currentCellPosition));
    }


    protected List<Creature> creaturesWithinRange = new List<Creature>();
    protected List<Creature> friendlyCreaturesWithinRange = new List<Creature>();
    List<Structure> structresWithinRange = new List<Structure>();

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
        if (targetToFollow != null && IsCreatureWithinRange(targetToFollow))
        {
            currentTargetedCreature = targetToFollow;
        }
        if (targetToFollow == null || !IsCreatureWithinRange(targetToFollow))
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


                if (creatureInRange.playerOwningCreature == this.playerOwningCreature)
                {
                    if (!friendlyCreaturesWithinRange.Contains(creatureInRange))
                    {
                        friendlyCreaturesWithinRange.Add(creatureInRange);
                    }
                }
            }
        }
        

        if (structureToFollow != null)
        {
            if (IsStructureInRange(structureToFollow))
            {
                currentTargetedStructure = structureToFollow;
                currentTargetedCreature = null;
            }
            else
            {
                currentTargetedStructure = null;
            }
        }
        if (currentTargetedStructure != null)
        {
            if (!IsStructureInRange(currentTargetedStructure))
            {
                currentTargetedStructure = null;
            }
        }
        foreach (Structure structureInRange in structresWithinRange)
        {
            if (structureInRange.playerOwningStructure != this.playerOwningCreature)
            {
                if (currentTargetedStructure == null && currentTargetedCreature == null)
                {
                    currentTargetedStructure = structureInRange;
                }
            }
        }

        #region cleanup
        if (currentTargetedStructure == null && currentTargetedCreature == null)
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
        }
        #endregion

    }


    [SerializeField] Transform visualAttackParticle;

    protected virtual void HandleFriendlyCreaturesList()
    {

    }

    public void HandleAttack()
    {
        CheckForCreaturesWithinRange();
        ChooseTarget();
        if (currentTargetedCreature != null)
        {
            VisualAttackAnimation(currentTargetedCreature);
            canAttack = false;
        }
        if (currentTargetedStructure != null)
        {
            VisualAttackAnimationOnStructure(currentTargetedStructure);
            canAttack = false;
        }
    }

    protected virtual void VisualAttackAnimation(Creature creatureToAttack)
    {
        if (visualAttackParticle != null)
        {
            if (range != 1)
            {
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.GetComponent<VisualAttackParticle>().SetTarget(creatureToAttack, Attack);
            }
            else
            {
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.transform.LookAt(creatureToAttack.transform);
                instantiatedParticle.GetComponent<MeleeVisualAttack>().SetTarget(creatureToAttack, Attack);
            }
            OnAttack();
        }
    }
    protected virtual void VisualAttackAnimationOnStructure(Structure structureToAttack)
    {
        if (visualAttackParticle != null)
        {
            if (range != 1)
            {
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.GetComponent<VisualAttackParticle>().SetTargetStructure(structureToAttack, Attack);
            }
            else
            {
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.transform.LookAt(structureToAttack.transform);
                instantiatedParticle.GetComponent<MeleeVisualAttack>().SetTargetStructure(structureToAttack, Attack);
            }
        }
    }

    public void TakeDamage(float attack)
    {
        this.CurrentHealth -= attack;
        GameManager.singleton.SpawnDamageText(new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), attack);
        UpdateCreatureHUD();
        if (this.CurrentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        lrGameObject.SetActive(false);
        lrGameObject2.SetActive(false);
        rangeLrGO.SetActive(false);
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
        this.attackText.text = Attack.ToString();
    }

    internal void OnTurn()
    {
        canAttack = true;
        HandleFriendlyCreaturesList();
        //GiveCounter(1);
    }
    void GiveCounter(int numOfCounters)
    {
        if (this.transform == null)
        {
            return;
        }
        for (int i = 0; i < numOfCounters; i++)
        {
            MaxHealth++;
            CurrentHealth++;
            Attack++;
        }
        GameManager.singleton.SpawnLevelUpPrefab(this.transform.position);
        UpdateCreatureHUD();
    }
    private void OnTick()
    {
    }

    [HideInInspector] public List<BaseTile> pathVectorList = new List<BaseTile>();
    int currentPathIndex;

    

    public virtual void SetMove(Vector3 positionToTarget)
    {
        rangeLr.enabled = false;
        playerOwningCreature.SetVisualsToNothingSelectedLocally();
        Vector3Int targetedCellPosition = grid.WorldToCell(new Vector3(positionToTarget.x, 0, positionToTarget.z));

        if (tempLineRendererBetweenCreatures != null)
        {
            tempLineRendererBetweenCreatures.enabled = false;
        }


        List<BaseTile> tempPathVectorList = pathfinder1.FindPath(currentCellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).tilePosition, thisTraversableType);
        if (tempPathVectorList == null) return;
        List<BaseTile> path = tempPathVectorList;
        pathVectorList = path;
        //SetNewTargetPosition(BaseMapTileState.singleton.GetWorldPositionOfCell(path[1].tilePosition));
        //SetNewTargetPosition(positionToTarget);
        SetLRPoints();
        currentPathIndex = 0;
        creatureState = CreatureState.Moving;

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
        if (pathVectorList.Count > 0)
        {
            targetedPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[currentPathIndex].tilePosition);

            if (Vector3.Distance(actualPosition, targetedPosition) > .02f)
            {
                actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedPosition.x, actualPosition.y, targetedPosition.z), speed * Time.fixedDeltaTime);
                SetLRPoints();
            }
            
            if (Vector3.Distance(actualPosition, targetedPosition) <= .02f)
            {
                if (targetToFollow != null)
                {
                    if (creaturesWithinRange.Contains(targetToFollow))
                    {
                        if (Vector3.Distance(actualPosition, targetedPosition) <= .02f)
                        {
                            SetStateToIdle();
                        }
                    }
                }

                if (targetToFollow != null)
                {
                    if (lastTileFollowCreatureWasOn == null)
                    {
                        lastTileFollowCreatureWasOn = targetToFollow.tileCurrentlyOn;
                    }


                    if (lastTileFollowCreatureWasOn != targetToFollow.tileCurrentlyOn)
                    {
                        SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetToFollow.tileCurrentlyOn.tilePosition));
                        lastTileFollowCreatureWasOn = targetToFollow.tileCurrentlyOn;

                    }
                }
                if (structureToFollow != null)
                {
                    if (structresWithinRange.Contains(structureToFollow))
                    {
                        if (Vector3.Distance(actualPosition, targetedPosition) <= .02f)
                        {
                            SetStateToIdle();
                        }
                    }
                }
                if (currentPathIndex >= pathVectorList.Count - 1)
                {
                    if (pathVectorList[currentPathIndex].CreatureOnTile() == null || pathVectorList[currentPathIndex].CreatureOnTile() == this)
                    {
                        SetStateToIdle();
                    }
                    else
                    {
                    }
                }
                else
                {
                    currentPathIndex++;
                }
            }
            if (tileCurrentlyOn == pathVectorList[0] && currentPathIndex == 0 && pathVectorList.Count > 1)
            {
                currentPathIndex++;
            }
            //Vector3Int targetedCellPosition = grid.WorldToCell(new Vector3(targetPosition.x, 0, targetPosition.z));
            currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
            if (BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition) == null)
            {
                tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            }
            if (previousTilePosition != tileCurrentlyOn)
            {
                CalculateAllTilesWithinRange();
                previousTilePosition.RemoveCreatureFromTile(this);
                previousTilePosition = tileCurrentlyOn;
                tileCurrentlyOn.AddCreatureToTile(this);
            }

            CheckForLastCreatureInPath();
            CheckForCreaturesInPath();
        }
        else
        {
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell( tileCurrentlyOn.tilePosition ));
        }
    }

    void DrawLine()
    {
        if (targetToFollow != null)
        {

            DrawLineToTargetedCreature(targetToFollow.actualPosition);
        }
        if (structureToFollow != null)
        {

            DrawLineToTargetedCreature(structureToFollow.transform.position);
        }
    }
    BaseTile lastTileFollowCreatureWasOn;
    private void CheckForFollowTarget()
    {
        if (targetToFollow == null)
        {
            if (tempLineRendererBetweenCreatures != null)
            {
                tempLineRendererBetweenCreatures.enabled = false;
            }
        }
        if (targetToFollow != null)
        {
            if (pathVectorList.Count > 0)
            {

                if (!pathVectorList[pathVectorList.Count - 1].neighborTiles.Contains(targetToFollow.tileCurrentlyOn))
                {
                    SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetToFollow.tileCurrentlyOn.tilePosition));
                }
            }
        }

        /*if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition).CreatureOnTile() != null)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition).CreatureOnTile() != this)
            {
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition).neighborTiles[1].tilePosition));
            }
        }
        if (targetToFollow != null)
        {
            if (!creaturesWithinRange.Contains(targetToFollow))
            {
                if (targetToFollow.tileCurrentlyOn.traverseType == BaseTile.traversableType.OnlyFlying)
                {
                    if (thisTraversableType == travType.Walking || thisTraversableType == travType.SwimmingAndWalking || thisTraversableType == travType.Swimming)
                    {
                        return;
                    }
                }
                if (targetToFollow.tileCurrentlyOn.traverseType == BaseTile.traversableType.SwimmingAndFlying)
                {
                    if (thisTraversableType == travType.Walking)
                    {
                        return;
                    }
                }
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetToFollow.tileCurrentlyOn.tilePosition));
            }
            if (targetToFollow.pathVectorList.Count > 0)
            {
                if (targetToFollow.pathVectorList.Count > targetToFollow.currentPathIndex)
                {
                    /*if (!allTilesWithinRange.Contains(targetToFollow.pathVectorList[targetToFollow.currentPathIndex + 1]))
                    {
                        SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(targetToFollow.tileCurrentlyOn.tilePosition));
                    }
                }
            }
        }*/
    }


    LineRenderer tempLineRendererBetweenCreatures;

    GameObject tempLineRendererBetweenCreaturesGameObject;
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
            tempPositions.Add(new Vector3( this.actualPosition.x, this.actualPosition.y + .1f, this.actualPosition.z ));
            tempPositions.Add(new Vector3(positionSent.x, positionSent.y + .1f, positionSent.z));
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


    private void CheckForLastCreatureInPath()
    {
        int i = pathVectorList.Count - 1;
        while (BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[i].tilePosition) != null && BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[i].tilePosition) != this)
        {
            i--;
        }
        if (i != pathVectorList.Count - 1)
        {
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[i].tilePosition));
        }
    }
    //its not recursive i swear
    private void CheckForCreaturesInPath()
    {
        if (currentPathIndex < pathVectorList.Count - 1)
        {
            if (BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex + 1].tilePosition) != null && BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex + 1].tilePosition) != this)
            {
                SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[pathVectorList.Count - 1].tilePosition));
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

    internal void SetToPlayerOwningCreature(Controller controller)
    {
        this.playerOwningCreature = controller;
        this.transform.GetComponent<MeshRenderer>().material.color = controller.col;
        colorIndicator.GetComponent<SpriteRenderer>().color = controller.col;
        ownedCreatureID = GameManager.singleton.creatureGuidCounter;
        GameManager.singleton.creatureGuidCounter++;
    }

    void SetStateToIdle()
    {
        tileCurrentlyOn.RemoveCreatureFromTile(this);
        lr.enabled = false;
        actualPosition = targetedPosition;
        this.transform.position = actualPosition;
        currentCellPosition = grid.WorldToCell(new Vector3(this.transform.position.x, 0, this.transform.position.z));
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        tileCurrentlyOn.AddCreatureToTile(this);
        creatureState = CreatureState.Idle;
    }

    #region range
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
        originalCard = cardSelected;
        originalCardTransform = Instantiate(cardSelected.transform, GameManager.singleton.canvasMain.transform);
        originalCardTransform.transform.position = this.transform.position;
        originalCardTransform.transform.localEulerAngles = Vector3.zero;
        originalCardTransform.transform.localScale = originalCardTransform.transform.localScale * 2f;

        originalCardTransform.GetComponentInChildren<BoxCollider>().enabled = false;
        originalCardTransform.gameObject.SetActive(false);

    }

    private void OnMouseOver()
    {
        originalCardTransform.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + .5f, this.transform.position.z);
        originalCardTransform.gameObject.SetActive(true);
        rangeLr.enabled = true;

        if (playerOwningCreature.locallySelectedCard != null)
        {
            if (playerOwningCreature.locallySelectedCard.cardType != CardInHand.CardType.Spell)
            {
                playerOwningCreature.locallySelectedCard.gameObject.SetActive(false);
            }
        }
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
    public virtual void OnETB() { }
    public virtual void OnDeath() { }
    public virtual void OnDamaged() { }
    public virtual void OnHealed() { }

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

    private void OnDestroy()
    {
        SetStateToDead();
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
        this.playerOwningCreature.creaturesOwned.Remove(this.ownedCreatureID);
        OnMouseExit();
        creatureState = CreatureState.Dead;
    }

    public Creature targetToFollow;
    internal void SetTargetToFollow(Creature creatureToFollow)
    {
        if (structureToFollow != null)
        {
            structureToFollow = null;
        }
        if (creatureToFollow != this)
        {
            targetToFollow = creatureToFollow;
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(creatureToFollow.tileCurrentlyOn.tilePosition));
        }
    }
    public Structure structureToFollow;
    internal void SetStructureToFollow(Structure structureToFollowSent)
    {
        if (targetToFollow != null)
        {
            targetToFollow = null;
        }
        structureToFollow = structureToFollowSent;
        SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(structureToFollow.tileCurrentlyOn.tilePosition));
    }


    #endregion

}
