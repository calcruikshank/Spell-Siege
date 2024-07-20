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


    [SerializeField] Transform highlightForCreatureSelected;

    protected Transform creatureImage;

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

    public SpellSiegeData.travType thisTraversableType;


    //LineRenderer lr;
    //public LineRenderer lr2;
    //GameObject lrGameObject;
    //GameObject lrGameObject2;

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



    protected virtual void SetTravType()
    {
    }
    protected virtual void Update()
    {
        switch (creatureState)
        {
            case CreatureState.Moving:
                VisualMove();
                break;
        }
        if (targetToFollow != null)
        {
            Vector3 targetRotation = new Vector3(targetToFollow.transform.position.x, transform.position.y, targetToFollow.transform.position.z) - this.transform.position;
            creatureImage.forward = Vector3.RotateTowards(creatureImage.forward, targetRotation, 10 * Time.deltaTime, 0);
        }
        if (structureToFollow != null)
        {
            Vector3 targetRotation = new Vector3(structureToFollow.transform.position.x, transform.position.y, structureToFollow.transform.position.z) - this.transform.position;
            creatureImage.forward = Vector3.RotateTowards(creatureImage.forward, targetRotation, 10 * Time.deltaTime, 0);
        }

        if (canAttackIcon != null)
        {
            canAttackIcon.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + .1f, this.transform.position.z);
            canAttackIcon.transform.localEulerAngles = new Vector3(creatureImage.localEulerAngles.x, creatureImage.localEulerAngles.y + 45, creatureImage.localEulerAngles.z);
        }

    }
    void FixedUpdate()
    {
        switch (creatureState)
        {
            case CreatureState.Moving:
                Move();
                //ChooseTarget();
                HandleAttackRate();
                CheckForCreaturesWithinRange();
                HandleAbilityRate();
                //DrawLine();
                ChooseTarget();
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Idle:
                animatorForObject.SetTrigger("Idle");
                ChooseTarget();
                //DrawLine();
                CheckForCreaturesWithinRange();
                HandleAttackRate();
                HandleAbilityRate();
                if (currentTargetedCreature != null)
                {
                    // Calculate the direction vector
                    Vector3 directionToTarget = currentTargetedCreature.transform.position - this.transform.position;

                    // Calculate the rotation required to look at the target
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    // Apply the rotation to your object
                    animatorForObject.transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                }
                //HandleFriendlyCreaturesList();
                //HandleAttack();
                break;
            case CreatureState.Summoned:
                ChooseTarget();
                //DrawLine();
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
    public Structure currentTargetedStructure;
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
    bool tauntFound = false;
    public virtual void ChooseTarget()
    {
        float lowestHealthCreatureWithinRange = -1;

        Creature closestCreature = null;
        float minDistance = float.MaxValue;
        tauntFound = false;
        foreach (Creature creatureWithinRange in creaturesWithinRange)
        {
            if (creatureWithinRange == null)
            {
                creaturesWithinRange.Remove(creatureWithinRange);
                return;
            }

            if (creatureWithinRange.playerOwningCreature != this.playerOwningCreature)
            {
                if (creatureWithinRange.taunt)
                {
                    currentTargetedCreature = creatureWithinRange;
                    tauntFound = true;
                    break; // Since we always want to target the creature with taunt, we can break the loop here
                }

                float distance = Vector3.Distance(this.transform.position, creatureWithinRange.transform.position);
                if (!tauntFound && distance < minDistance)
                {
                    minDistance = distance;
                    closestCreature = creatureWithinRange;
                }
            }
        }

        if (!tauntFound && closestCreature != null)
        {
            currentTargetedCreature = closestCreature;
        }
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



        if (currentTargetedCreature != null && IsCreatureWithinRange(currentTargetedCreature) && Vector3.Distance(actualPosition, new Vector3(tileCurrentlyOn.transform.position.x, this.transform.position.y, tileCurrentlyOn.transform.position.z)) < .1f)
        {
            creatureState = CreatureState.Idle;
            return;
        }
        if (currentTargetedStructure != null && IsStructureInRange(currentTargetedStructure))
        {
            //creatureState = CreatureState.Idle;
        }

        if (currentTargetedCreature != null && !IsCreatureWithinRange(currentTargetedCreature))
        {
            //creatureState = CreatureState.Moving;
        }
        if (currentTargetedStructure != null && !IsStructureInRange(currentTargetedStructure) && targetedCellForChoosingTargets.traverseType != SpellSiegeData.traversableType.Untraversable && targetedCellForChoosingTargets.CreatureOnTile() == null && targetedCellForChoosingTargets.structureOnTile == null)
        {
            //&&
            creatureState = CreatureState.Moving;
        }

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
                //OnAttack();
                animatorForObject.SetTrigger("Attack");
                return;
            }
        }
        if (currentTargetedStructure != null)
        {
            if (IsStructureInRange(currentTargetedStructure))
            {
                VisualAttackAnimationOnStructure(currentTargetedStructure);
                canAttack = false;
                //OnAttack();
                animatorForObject.SetTrigger("Attack");
                return;
            }

            if (targetedCellForChoosingTargets != null)
            {
                if (targetedCellForChoosingTargets.traverseType == SpellSiegeData.traversableType.Untraversable)
                {
                    VisualAttackAnimationOnStructure(currentTargetedStructure);
                    canAttack = false;
                    //OnAttack();
                    animatorForObject.SetTrigger("Attack");
                    return;
                }
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

                canAttackIcon.GetComponent<SpawnAnimatedSword>().SpawnSword(creatureToAttack.transform);
                canAttackIcon.gameObject.SetActive(false);
                Transform instantiatedParticle = Instantiate(visualAttackParticle, new Vector3(this.transform.position.x, this.transform.position.y - .1f, this.transform.position.z), Quaternion.identity);
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
            canAttackIcon.GetComponent<SpawnAnimatedSword>().SpawnSword(structureToAttack.transform);
            canAttackIcon.gameObject.SetActive(false);
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
        rangeLrGO.SetActive(false);
        OnDeath();
        GameManager.singleton.CreatureDied(this.creatureID);

        if (canAttackIcon != null)
        {
            canAttackIcon.gameObject.SetActive(false);
        }
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

    public virtual void OnTurn()
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



    public virtual void SetMove(Vector3 positionToTarget, Vector3 originalPosition)
    {
        actualPosition = originalPosition;
        HidePathfinderLR();
        rangeLr.enabled = false;

        if (tempLineRendererBetweenCreatures != null)
        {
            tempLineRendererBetweenCreatures.enabled = false;
        }

        targetedPosition = positionToTarget;

        //currentCellPosition = grid.WorldToCell(new Vector3(actualPosition.x, 0, actualPosition.z));
        //List<BaseTile> tempPathVectorList = pathfinder1.FindPath(tileCurrentlyOn.tilePosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(targetedCellPosition).tilePosition, thisTraversableType);
        //List<BaseTile> path = tempPathVectorList;
        //pathVectorList = path;
        //SetNewTargetPosition(BaseMapTileState.singleton.GetWorldPositionOfCell(path[1].tilePosition));
        //SetNewTargetPosition(positionToTarget);
        creatureState = CreatureState.Moving;

        animatorForObject.SetTrigger("Run");

    }

    public void SetMoveRpc()
    {
    }


    public Animator animatorForObject;
    public BaseTile targetedCell;
    public BaseTile targetedCellForChoosingTargets;
    public virtual void Move()
    {
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
        if (currentTargetedStructure != null)
        {
            targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            if (currentTargetedStructure.currentCellPosition.x < this.currentCellPosition.x)
            {
                targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - 1, currentCellPosition.y, currentCellPosition.z));
                targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x - 1, currentCellPosition.y, currentCellPosition.z));

                animatorForObject.transform.localEulerAngles = new Vector3(0, -90, 0);
            }
            else
            {
                targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + 1, currentCellPosition.y, currentCellPosition.z));
                targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(currentCellPosition.x + 1, currentCellPosition.y, currentCellPosition.z));

                animatorForObject.transform.localEulerAngles = new Vector3(0, 90, 0);
            }



            if (targetedCell.CreatureOnTile() != null || targetedCell.structureOnTile != null || targetedCell.traverseType == SpellSiegeData.traversableType.Untraversable)
            {
                targetedCell = tileCurrentlyOn;

                bool creatureCanMove = false;
                if (Vector3.Distance(actualPosition, new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z)) < .01f)
                {
                    if (targetedCell.CreatureOnTile() != null)
                    {
                        if (targetedCell.CreatureOnTile().playerOwningCreature == this.playerOwningCreature && targetedCell.CreatureOnTile().creatureState == CreatureState.Moving)
                        {
                            creatureCanMove = true;
                        }
                    }
                    if (!creatureCanMove)
                    {
                        //SetStateToIdle();
                    }
                    SetStateToIdle();
                }
            }
            animatorForObject.SetTrigger("Run");
            actualPosition = Vector3.MoveTowards(actualPosition, new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z), speed * Time.fixedDeltaTime);

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


    protected void VisualMove()
    {
        this.transform.position = actualPosition;
        //Vector3 targetRotation = targetedPosition - this.transform.position;
        //creatureImage.forward = Vector3.RotateTowards(creatureImage.forward, targetRotation, 10 * Time.deltaTime, 0);


    }

    public Transform canAttackIcon;
    internal void SetToPlayerOwningCreature(Controller controller)
    {
        this.playerOwningCreature = controller;
        animatorForObject = transform.GetComponentInChildren<Animator>();

        Outline outline = animatorForObject.gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = animatorForObject.gameObject.AddComponent<Outline>();
        }
        outline.OutlineColor = controller.col;
        outline.OutlineWidth = 2.5f;
        grid = GameManager.singleton.grid;
        baseTileMap = GameManager.singleton.baseMap;
        animatorForObject.SetTrigger("Run");
        creatureImage = this.transform.GetChild(0);

        SetRangeLineRenderer();

        SetTravType();
        //pathfinder1 = new Pathfinding();
        //pathfinder2 = new Pathfinding();
        UpdateCreatureHUD();
        currentCellPosition = grid.WorldToCell(this.transform.position);
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        previousTilePosition = tileCurrentlyOn;
        tileCurrentlyOn.AddCreatureToTile(this);
        actualPosition = this.transform.position;

        CalculateAllTilesWithinRange();
        creatureState = CreatureState.Summoned;
        creatureID = GameManager.singleton.allCreatureGuidCounter;
        GameManager.singleton.allCreaturesOnField.Add(creatureID, this);
        GameManager.singleton.allCreatureGuidCounter++;
        //this.transform.GetComponent<MeshRenderer>().material.color = controller.col;
        //colorIndicator.GetComponent<SpriteRenderer>().color = controller.col;
        canAttack = true;
        canAttackIcon = Instantiate(GameManager.singleton.canAttackIcon, this.transform.position, Quaternion.identity);
        //canAttackIcon.parent = transform;
        canAttackIcon.localScale = new Vector3(.3f, .3f, .3f);
        canAttackIcon.position = new Vector3(0, 0f, 0);
        canAttackIcon.localEulerAngles = new Vector3(0, -45, 0);
        canAttackIcon.gameObject.SetActive(true);
    }

    public void SetStateToIdle()
    {
        if (playerOwningCreature.IsOwner)
        {
            //playerOwningCreature.SetCreatureToIdleServerRpc(creatureID, currentCellPosition);
        }
        tileCurrentlyOn.RemoveCreatureFromTile(this);

        HidePathfinderLR();
        this.actualPosition = BaseMapTileState.singleton.GetWorldPositionOfCell(currentCellPosition);
        this.transform.position = actualPosition;
        currentCellPosition = grid.WorldToCell(new Vector3(this.actualPosition.x, 0, this.actualPosition.z));
        tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
        tileCurrentlyOn.AddCreatureToTile(this);
        creatureState = CreatureState.Idle;
        animatorForObject.SetTrigger("Idle");
    }

    #region range


    internal void LocalSetCreatureToIdle(Vector3Int actualPositionSent)
    {
        if (!playerOwningCreature.IsOwner)
        {
            tileCurrentlyOn.RemoveCreatureFromTile(this);

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
    public void CalculateAllTilesWithinRange()
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
        //List<BaseTile> tempPathVectorList = pathfinder2.FindPath(currentCellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(hoveredTilePosition).tilePosition, thisTraversableType);
        List<Vector3> lrList = new List<Vector3>();
        //targetPosition = positionToTarget;

    }
    internal void HidePathfinderLR()
    {
        if (positions == null)
        {
            positions = new Vector3[2];
        }
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
        this.MaxHealth = cardSelected.currentHealth;
        this.CurrentAttack = cardSelected.currentAttack;
        this.CurrentHealth = cardSelected.currentHealth;
        this.creatureType = cardSelected.creatureType;
        Debug.Log("Setting original card to " + cardSelected);
        originalCard = cardSelected;
        originalCardTransform = Instantiate(cardSelected.transform, GameManager.singleton.scalableUICanvas.transform);
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        originalCardTransform.transform.localEulerAngles = Vector3.zero;
        originalCardTransform.transform.localScale = originalCardTransform.transform.localScale * 2f;

        originalCardTransform.GetComponentInChildren<BoxCollider>().enabled = false;
        originalCardTransform.gameObject.SetActive(false);
        UpdateCreatureHUD();

    }

    private void OnMouseOver()
    {
        rangeLr.enabled = true;
        if (playerOwningCreature.locallySelectedCard != null)
        {
            if (playerOwningCreature.locallySelectedCard.cardType != SpellSiegeData.CardType.Spell)
            {
                playerOwningCreature.locallySelectedCard.gameObject.SetActive(false);
            }
        }
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);

        originalCardTransform.transform.localScale = Vector3.one * 200 / originalCardTransform.transform.position.z;
        originalCardTransform.gameObject.SetActive(true);

    }

    internal void VisuallySelect()
    {
        highlightForCreatureSelected.gameObject.SetActive(true);
    }
    internal void VisuallyDeSelect()
    {
        highlightForCreatureSelected.gameObject.SetActive(false);
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
            currentTargetedStructure = structureToFollowSent;
            if (IsStructureInRange(structureToFollowSent))
            {
                return;
            }
            SetMove(BaseMapTileState.singleton.GetWorldPositionOfCell(structureToFollowSent.tileCurrentlyOn.tilePosition), originalCreaturePosition);
        }
    }



    #endregion

}
