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
    [SerializeField] float Attack;
    [SerializeField] float AttackRate;
    float AttackRateTimer;
    float CurrentHealth;
    [SerializeField] float MaxHealth;

    [SerializeField] TextMeshPro attackText;
    [SerializeField] TextMeshPro healthText;

    [SerializeField] int numOfTargetables = 1;

    [HideInInspector] public List<BaseTile> allTilesWithinRange;
    [HideInInspector] public int creatureID;
    [HideInInspector] public int ownedCreatureID;
    [HideInInspector] public CreatureState creatureState;
    [HideInInspector]
    public enum CreatureState
    {
        Summoned, //On The turn created
        Moving,
        Idle
        //not sure if i need a tapped state yet trying to keep it as simple as possible
    }

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
    [HideInInspector] public travType thisTraversableType;


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
    Vector3[] positions = new Vector3[2];


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
        thisTraversableType = travType.Walking;
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
                HandleAttackRate();
                HandleAttack();
                break;
            case CreatureState.Idle:
                CheckForCreaturesWithinRange();
                HandleAttackRate();
                HandleAttack();
                break;
            case CreatureState.Summoned:
                CheckForCreaturesWithinRange();
                HandleAttackRate();
                HandleAttack();
                break;
        }
    }

    List<Creature> creaturesWithinRange = new List<Creature>();
    List<Creature> currentTargetedCreature = new List<Creature>();
    private void CheckForCreaturesWithinRange()
    {
        float lowestHealthCreatureWithinRange = -1;
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
        foreach (Creature creatureInRange in creaturesWithinRange)
        {
            if (creatureInRange == null)
            {
                creaturesWithinRange.Remove(creatureInRange);
                currentTargetedCreature = new List<Creature>();
                return;
            }
            if (!allTilesWithinRange.Contains(creatureInRange.tileCurrentlyOn))
            {
                creaturesWithinRange.Remove(creatureInRange);
                currentTargetedCreature = new List<Creature>();
                return;
            }
        }
        foreach (Creature creatureInRange in creaturesWithinRange)
        {
            if (creatureInRange.playerOwningCreature != this.playerOwningCreature)
            {
                if (lowestHealthCreatureWithinRange == -1 || creatureInRange.CurrentHealth < lowestHealthCreatureWithinRange)
                {
                    lowestHealthCreatureWithinRange = creatureInRange.CurrentHealth;
                    if (currentTargetedCreature.Count < numOfTargetables)
                    {
                        currentTargetedCreature.Add(creatureInRange);

                    }
                }
            }
        }

    }


    [SerializeField] Transform visualAttackParticle;

    void HandleAttack()
    {
        if (currentTargetedCreature.Count > 0)
        {
            if (AttackRateTimer >= AttackRate)
            {
                AttackRateTimer = 0;
                for (int i = 0; i < currentTargetedCreature.Count; i++)
                {
                    VisualAttackAnimation(currentTargetedCreature[i]);
                    //AttackCreature(currentTargetedCreature[i]);
                }
            }
        }

    }
    void VisualAttackAnimation(Creature creatureToAttack)
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
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y + .2f, this.transform.position.z), Quaternion.identity);
                instantiatedParticle.transform.LookAt(creatureToAttack.transform);
                instantiatedParticle.GetComponent<MeleeVisualAttack>().SetTarget(creatureToAttack, Attack);
            }
        }
    }

    public void TakeDamage(float attack)
    {
        this.CurrentHealth -= attack;
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
        AttackRateTimer += Time.fixedDeltaTime;

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

    private void OnTick()
    {
    }

    [HideInInspector] public List<BaseTile> pathVectorList = new List<BaseTile>();
    int currentPathIndex;

    public virtual void SetMove(Vector3 positionToTarget)
    {
        HidePathfinderLR();
        Vector3Int targetedCellPosition = grid.WorldToCell(new Vector3(positionToTarget.x, 0, positionToTarget.z));

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

        lrList.Add(new Vector3(this.transform.position.x, BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[0].tilePosition).y, this.transform.position.z));
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
            targetedPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[currentPathIndex].tilePosition);

            if (Vector3.Distance(actualPosition, targetedPosition) > .02f)
            {
                actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedPosition.x, actualPosition.y, targetedPosition.z), speed * Time.fixedDeltaTime);
                SetLRPoints();
            }
            if (Vector3.Distance(actualPosition, targetedPosition) <= .02f)
            {
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

        if (BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex].tilePosition) != null)
        {
            if (currentPathIndex == pathVectorList.Count - 1 && BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex].tilePosition) != this)
            {
                if (pathVectorList[currentPathIndex - 1] != null)
                {
                    if (pathVectorList[currentPathIndex - 1].CreatureOnTile() == this)
                    {
                        SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[currentPathIndex - 1].tilePosition));
                        return;
                    }
                }
                foreach (BaseTile neighbor in pathVectorList[currentPathIndex].neighborTiles)
                {
                    if (neighbor.CreatureOnTile() == null)
                    {
                        //SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(neighbor.tilePosition));
                    }
                }
                return;
            }
            //SetNewTargetPosition(BaseMapTileState.singleton.GetWorldPositionOfCell(currentCellPosition));
            if (BaseMapTileState.singleton.GetCreatureAtTile(pathVectorList[currentPathIndex].tilePosition) != this)
            {
                for (int i = pathVectorList.Count - 1; i > currentPathIndex; i--)
                {
                    if (pathVectorList[i])
                    {
                        if (pathVectorList[i].CreatureOnTile() == null)
                        {
                            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(pathVectorList[i].tilePosition));
                            break;
                        }
                    }
                    //pathVectorList.RemoveAt(pathVectorList.Count - 1);
                    //BaseTile newBaseTileToMoveTo = BaseMapTileState.singleton.GetNearestBaseTileGivenCell(tileCurrentlyOn, BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition));
                    //SetNewTargetPosition(BaseMapTileState.singleton.GetWorldPositionOfCell(newBaseTileToMoveTo.tilePosition));
                }
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
        colorIndicator.GetComponent<SpriteRenderer>().color = controller.col;
        ownedCreatureID = GameManager.singleton.creatureGuidCounter;
        GameManager.singleton.creatureGuidCounter++;
    }

    void SetStateToIdle()
    {
        tileCurrentlyOn.RemoveCreatureFromTile(this);
        lr.enabled = false;
        Debug.Log("setting state to idle and tick " + playerOwningCreature.tick);
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

    private void OnMouseOver()
    {
        rangeLr.enabled = true;
    }

    private void OnMouseExit()
    {
        rangeLr.enabled = false;
    }


    #endregion

    #region Overridables
    public virtual void Garrison() { }
    public virtual void OnETB() { }
    public virtual void OnDeath() { }
    public virtual void OnDamaged() { }
    public virtual void OnHealed() { }
    public virtual void Taunt() { }
    public virtual void Heal() { }


    #endregion

}
