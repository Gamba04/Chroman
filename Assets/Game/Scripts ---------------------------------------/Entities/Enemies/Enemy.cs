using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour, IHittable
{
    public enum EnemyState
    {
        Idle,
        Wandering,
        Returning,
        Pursuit,
        Attacking,
        Searching,
        Dash,
        None
    }

    [Header("Components")]
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    private SpriteRenderer sr;
    [SerializeField]
    protected Animator anim;
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    protected new Collider2D collider;
    [SerializeField]
    protected new SpriteRenderer light;
    [SerializeField]
    protected Ghosting ghosting;
    [SerializeField]
    private SpriteRenderer[] whiteSprites;

    [Header("Settings")]
    [SerializeField]
    private bool rotateWithDirection;
    [SerializeField]
    protected Sprite ghostingSprite;
    [SerializeField]
    protected float attackMaxCooldown;
    [SerializeField]
    protected float attackPrewarm = 0.5f;
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField]
    private float noMovementMaxCooldown;
    [SerializeField]
    private UnityEvent onDeadEvent;
    [SerializeField]
    protected Color lightDefaultColor = Color.white;
    [SerializeField]
    private int healSpawns;
    [SerializeField]
    private float healSpawnRange = 0.25f;
    [GambaHeader("Movement")]
    [SerializeField]
    private float moveAcceleration = 100;
    [SerializeField]
    private float moveDeacceleration = 10;
    [SerializeField]
    private float moveSpeed = 6;
    [SerializeField]
    [Range(0, 1)]
    private float moveSteadyTreshold;
    [SerializeField]
    [Range(0, 1)]
    private float externalFriction;
    [SerializeField]
    protected float dashSpeed;
    [SerializeField]
    private float dashDuration;
    [GambaHeader("States")]
    [SerializeField]
    private float wanderingRange;
    [SerializeField]
    private float pursuingRange;
    [SerializeField]
    private float attackingRange;
    [Space()]
    [SerializeField]
    private bool debugs;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    protected EnemyState state;
    [ReadOnly, SerializeField]
    protected bool lockStateChange;
    [ReadOnly, SerializeField]
    protected Player player;
    [ReadOnly, SerializeField]
    private float distanceToPlayer;
    [ReadOnly, SerializeField]
    private bool isInLineOfSight;
    [ReadOnly, SerializeField]
    private bool isMoving;
    [ReadOnly, SerializeField]
    private float velocityMagnitude;
    [ReadOnly, SerializeField]
    private Vector2 velocity;
    [ReadOnly, SerializeField]
    protected bool dead;
    [GambaHeader("Inside of Wall", 0.7f)]
    [ReadOnly, SerializeField]
    private bool isInsideOfSolidWall;
    [ReadOnly, SerializeField]
    private bool isInsideOfWall;
    [GambaHeader("Attack Cooldown", 0.7f)]
    [ReadOnly, SerializeField]
    protected float attackCooldown;
    [ReadOnly, SerializeField]
    protected float subAttackCooldown;
    [GambaHeader("Stats", 1f, 0.3f, 0.2f, 0.7f)]
    [ReadOnly, SerializeField]
    private float health = 6;
    [ReadOnly, SerializeField]
    protected float damage;
    [GambaHeader("Pathfinding", 1f, 0.1f, 0.2f, 0.7f)]
    [ReadOnly, SerializeField]
    private float noMovementCooldown;
    [ReadOnly, SerializeField]
    private int currentNode;
    [ReadOnly, SerializeField]
    private List<Vector2> nodes;
    [ReadOnly, SerializeField]
    private Vector2 target;

    private Vector2 initialPos;
    private Vector2 lastPlayerPos;

    private Vector2 externalMomentum;

    private bool noMovementReset;
    private Vector2 noMovementLastPosition;

    private float deathCounter;
    private float deathDuration;

    protected float dashCooldown;
    protected Vector2 dashDirection;

    private Vector2 direction;

    private Vector2 lastPosition;
    private Vector2 lastValidPosition;

    private bool isActiveOnDistance;

    private bool lastInsideSolidWall;

    protected virtual void Start()
    {
        StartCoroutine(IsActiveOnDistanceLoop());
        SetActiveOnDistance(isActiveOnDistance);

        player = GameManager.Player;

        if (initialPos == Vector2.zero)
        {
            initialPos = transform.position;
        }

        deathDuration = explosion.duration;

        ChangeState(EnemyState.Wandering);

        light.color = lightDefaultColor;

        if (ghosting != null)
        {
            ghosting.SetParent(GameManager.ParentGhosting);
        }
    }

    private void Update()
    {
        if (!GameManager.GamePaused)
        {
            if (player != null)
            {
                if (!dead)
                {
                    if (isActiveOnDistance)
                    {
                        CheckIsInsideOfWall();
                        InsideWallDebugUpdate();

                        UpdateVelocity();

                        StateControlUpdate();
                        MainCooldownsUpdate();

                        ExternalMomentumUpdate();
                        LookRotationUpdate();

                        OnAliveUpdate();
                    }
                    else
                    {
                        Move(Vector2.zero);
                    }
                }
                else
                {
                    DeathUpdate();
                }
            }
            else
            {
                Debug.LogError("Player not found");
            }
        }
    }

    #region StateControl

    private void StateControlUpdate()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 position = transform.position;
        float distance = (playerPos - position).magnitude;
        distanceToPlayer = distance;

        switch (state)
        {
            case EnemyState.Idle:
                Idle();
                break;
            // ------------------------------------------------
            case EnemyState.Wandering:
                isInLineOfSight = IsInLineOfSight(playerPos);
                IsMovingUpdate();

                if (distance <= pursuingRange && isInLineOfSight && isMoving && !player.Dead)
                {
                    ChangeState(EnemyState.Pursuit);
                    WanderingExit();
                }
                else if ((initialPos - position).magnitude > wanderingRange && isMoving)
                {
                    ChangeStateOnDelay(EnemyState.Returning, 0.5f);
                    WanderingExit();
                }
                else
                {
                    Wandering();
                }
                break;
            // ------------------------------------------------
            case EnemyState.Returning:
                isInLineOfSight = IsInLineOfSight(playerPos);

                IsMovingUpdate();

                if (distance <= pursuingRange && isInLineOfSight)
                {
                    ChangeState(EnemyState.Pursuit);
                    ReturningExit();
                }
                else if ((initialPos - position).magnitude <= 0.1f)
                {
                    ChangeState(EnemyState.Wandering);
                    ReturningExit();
                }
                else
                {
                    if (isMoving)
                    {
                        Returning();
                    }
                    else
                    {
                        ChangeStateOnDelay(EnemyState.Wandering, 0.1f);
                        ReturningExit();
                    }
                }
                break;
            // ------------------------------------------------
            case EnemyState.Pursuit:
                isInLineOfSight = IsInLineOfSight(playerPos);
                IsMovingUpdate();

                if (!player.Dead)
                {
                    if (distance <= attackingRange)
                    {
                        ChangeStateOnDelay(EnemyState.Attacking, 0.2f);
                        PursuitExit();
                    }
                    else if (distance > pursuingRange)
                    {
                        ChangeStateOnDelay(EnemyState.Searching, 0.5f);
                        PursuitExit();
                    }
                    else
                    {
                        if (isInLineOfSight)
                        {
                            lastPlayerPos = playerPos;
                            if (isMoving)
                            {
                                Pursuit();
                            }
                            else
                            {
                                ChangeStateOnDelay(EnemyState.Wandering, 0.1f);
                                PursuitExit();
                            }
                        }
                        else
                        {
                            ChangeStateOnDelay(EnemyState.Searching, 0.3f);
                            PursuitExit();
                        }
                    }
                }
                else
                {
                    ChangeStateOnDelay(EnemyState.Wandering, 0.3f);
                    PursuitExit();
                }
                break;
            // ------------------------------------------------
            case EnemyState.Attacking:
                isInLineOfSight = IsInLineOfSight(playerPos);

                if (!player.Dead)
                {
                    if (isInLineOfSight)
                    {
                        if (distance > attackingRange)
                        {
                            if (subAttackCooldown <= 0)
                            {
                                ChangeState(EnemyState.Pursuit);
                                AttackingExit();
                            }
                            else
                            {
                                Attacking();
                            }
                        }
                        else
                        {
                            lastPlayerPos = playerPos;
                            Attacking();
                        }
                    }
                    else
                    {
                        if (subAttackCooldown <= 0)
                        {
                            ChangeState(EnemyState.Searching);
                            AttackingExit();
                        }
                        else
                        {
                            Attacking();
                        }
                    }
                }
                else
                {
                    ChangeStateOnDelay(EnemyState.Wandering, 0.3f);
                    AttackingExit();
                }
                break;
            // ------------------------------------------------
            case EnemyState.Searching:
                isInLineOfSight = IsInLineOfSight(playerPos);
                IsMovingUpdate();

                if (!player.Dead)
                {
                    if ((lastPlayerPos - (Vector2)transform.position).magnitude > 0.2f)
                    {
                        if (distance <= pursuingRange)
                        {
                            if (isInLineOfSight)
                            {
                                if (distance <= attackingRange)
                                {
                                    ChangeStateOnDelay(EnemyState.Attacking, 0.2f);
                                    SearchingExit();
                                }
                                else
                                {
                                    ChangeState(EnemyState.Pursuit);
                                    SearchingExit();
                                }
                            }
                            else
                            {
                                if (isMoving)
                                {
                                    Searching();
                                }
                                else
                                {
                                    ChangeStateOnDelay(EnemyState.Wandering, 0.1f);
                                    SearchingExit();
                                }
                            }
                        }
                        else
                        {
                            if (isMoving)
                            {
                                Searching();
                            }
                            else
                            {
                                ChangeStateOnDelay(EnemyState.Wandering, 0.1f);
                                SearchingExit();
                            }
                        }
                    }
                    else
                    {
                        ChangeStateOnDelay(EnemyState.Wandering, 0.5f);
                        SearchingExit();
                    }
                }
                else
                {
                    ChangeStateOnDelay(EnemyState.Wandering, 0.3f);
                    AttackingExit();
                }
                break;
            // ------------------------------------------------
            case EnemyState.Dash:
                Dashing();
                break;
        }
    }

    protected void ChangeState(EnemyState state)
    {
        if (!lockStateChange)
        {
            this.state = state;
        }

        if (debugs) print(state);
    }

    protected void ChangeStateOnDelay(EnemyState state, float delay)
    {
        ChangeState(EnemyState.Idle);

        Timer.CallOnDelay(() =>
        {
            ChangeState(state);
        }, delay, $"{GetType().Name}: Change state to {state.ToString()}");

        if (debugs) print(delay);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    #region States

    private void Idle()
    {
        Move(Vector2.zero);
    }
    private void Wandering()
    {
        if (IsInLineOfSight(target) && (target - (Vector2)transform.position).magnitude <= 5 && (isMoving || !noMovementReset) )
        {
            if ((target - (Vector2)transform.position).magnitude > 0.7f)
            {
                Move(target - (Vector2)transform.position);
            }
            else
            {
                target = -target;
                if (!(IsInLineOfSight(target) && (target - (Vector2)transform.position).magnitude <= 5))
                {
                    target = RandomPoint(1, 4);
                }

                ChangeStateOnDelay(EnemyState.Wandering, 1f);
            }
        }
        else
        {
            target = RandomPoint(1, 4);
        }
        Debug.DrawLine(target, transform.position);
    }

    private void Returning()
    {
        if (IsInLineOfSight(initialPos))
        {
            Move((initialPos - (Vector2)transform.position).normalized);
        }
        else
        {
            PathFindToPoint(initialPos);
            Debug.DrawLine(initialPos + Vector2.left, initialPos + Vector2.right);
            Debug.DrawLine(initialPos + Vector2.down, initialPos + Vector2.up);
        }
    }
    private void Pursuit()
    {
        Move((player.transform.position - transform.position).normalized);
    }

    private void Attacking()
    {
        Move(Vector2.zero);

        if (attackCooldown <= 0)
        {
            Attack();
            attackCooldown = attackMaxCooldown;
        }

        Timer.ReduceCooldown(ref subAttackCooldown, Hit);

        direction = (player.transform.position - transform.position);
    }

    private void Searching()
    {
        if (IsInLineOfSight(lastPlayerPos))
        {
            Move((lastPlayerPos - (Vector2)transform.position).normalized);
        }
        else
        {
            PathFindToPoint(lastPlayerPos);
            Debug.DrawLine(lastPlayerPos + Vector2.left, lastPlayerPos + Vector2.right);
            Debug.DrawLine(lastPlayerPos + Vector2.down, lastPlayerPos + Vector2.up);
        }
    }

    protected virtual void Dashing()
    {
        Timer.ReduceCooldown(ref dashCooldown, ()=>
        {
            DashExit();
            ChangeState(EnemyState.Wandering);
        });

        rb.velocity = dashDirection.normalized * dashSpeed;
    }

    //-----------------------------------------------------------------------------------------------

    private void WanderingExit()
    {
        target = transform.position;
    }

    private void ReturningExit()
    {
        currentNode = 0;
        nodes = null;
    }

    private void PursuitExit()
    {

    }

    private void AttackingExit()
    {

    }

    private void SearchingExit()
    {

    }

    protected virtual void DashExit()
    {
        ghosting.Disable();
        attackCooldown = attackMaxCooldown;
        subAttackCooldown = 0;
    }

    protected void AnyStateExit()
    {
        switch (state)
        {
            case EnemyState.Attacking:
                AttackingExit();
                break;
            case EnemyState.Dash:
                DashExit();
                break;
            case EnemyState.Pursuit:
                PursuitExit();
                break;
            case EnemyState.Returning:
                ReturningExit();
                break;
            case EnemyState.Searching:
                SearchingExit();
                break;
            case EnemyState.Wandering:
                WanderingExit();
                break;
        }
    }

    #endregion

    //-----------------------------------------------------------------------------------------------

    protected virtual void StartDash(Vector2 direction)
    {
        if (dashDuration > 0)
        {
            ChangeState(EnemyState.Dash);
            dashCooldown = dashDuration;

            ghosting.Enable(ghostingSprite, light.color);
            rb.velocity = direction.normalized * dashSpeed;
            dashDirection = direction.normalized;
        }
        else
        {
            DashExit();
            ChangeState(EnemyState.Wandering);
        }
    }

    protected virtual void Attack()
    {
        anim.SetBool("Cancel", false);
        anim.SetTrigger("Attack");

        subAttackCooldown = attackPrewarm;
    }

    protected virtual void Hit() { }

    private void Move(Vector2 direction)
    {
        direction = direction.normalized;

        // Acceleration
        if (direction.magnitude != 0)
        {
            velocity += direction * moveAcceleration * Time.deltaTime;
        }

        // Deacceleration
        if (velocity.magnitude > moveSteadyTreshold)
        {
            if (direction.magnitude == 0)
            {
                velocity *= 1 - Time.deltaTime * moveDeacceleration;
            }
        }
        else
        {
            velocity = Vector2.zero;
        }

        // Speed Limit
        if (velocity.magnitude > moveSpeed)
        {
            velocity = velocity.normalized * moveSpeed;
        }

        rb.velocity = velocity + externalMomentum;

        if (direction.magnitude != 0)
        {
            this.direction = direction;
        }
    }

    private void ExternalMomentumUpdate()
    {
        if (externalMomentum.sqrMagnitude > 0 && externalFriction > 0)
        {
            externalMomentum *= 1 - externalFriction * Time.deltaTime * 10;
        }
    }

    private void PathFindToPoint(Vector2 point)
    {
        if (nodes == null || nodes.Count == 0)
        {
            currentNode = 0;

            nodes = Pathfinder.FindPath(transform.position, point, collisionLayers: LayerMask.GetMask("Default", "Map", "Hittable"));

            if (nodes != null && nodes.Count > 0)
            {
                if ((nodes[nodes.Count - 1] - (Vector2)transform.position).magnitude < 0.1f)
                {
                    // Pathfind bug
                    nodes = null;
                }
            }

            if (nodes == null || nodes.Count == 0)
            {
                nodes = new List<Vector2>();
                nodes.Add(point);

                Timer.CallOnDelay(() =>
                {
                    nodes = null;
                }, 3);
            }
        }
        else // Pathfind active
        {
            if (currentNode < nodes.Count)
            {
                Vector2 target = nodes[currentNode];

                if ((target - (Vector2)transform.position).magnitude > 0.2f)
                {
                    Move(target - (Vector2)transform.position);
                }
                else
                {
                    currentNode++;
                }
            }
            else
            {
                currentNode = 0;
                nodes.Clear();
            }

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Debug.DrawLine(nodes[i], nodes[i + 1], (i == currentNode) ? new Color(1, 1, 1) : new Color(1, 1, 1, 0.2f));
            }
        }
    }

    private void LookRotationUpdate()
    {
        if (rotateWithDirection)
        {
            sr.transform.rotation = Quaternion.Slerp(sr.transform.rotation, Quaternion.LookRotation(Vector3.forward, direction), 0.2f * Time.deltaTime / 0.02f);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Death

    private void DeathUpdate()
    {
        Timer.ReduceCooldown(ref deathCounter, () =>
        {
            Destroy(gameObject);
        });

        float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }

    protected virtual void Die()
    {
        explosion.Emit(20);

        // Turn off components
        sr.enabled = false;
        collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;

        foreach (SpriteRenderer w in whiteSprites)
        {
            w.enabled = false;
        }

        // Add set death sentence
        deathCounter = deathDuration;
        dead = true;

        onDeadEvent?.Invoke();

        GameManager.SpawnHealsAtPos(healSpawns, transform.position, healSpawnRange);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region IHittable

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        if (!isInsideOfSolidWall)
        {
            if (state == EnemyState.Dash && layer != 14 && layer != 10)
            {
                DashExit();
                ChangeState(EnemyState.Attacking);
            }

            anim.SetBool("Cancel", false);
            anim.SetTrigger("Hit");
            externalMomentum = ((Vector2)transform.position - hitterPosition).normalized * knockback;

            AddHealth(-damage);
            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyHit);
        }
    }

    public bool AvoidingLayer(int layer)
    {
        return false;
    }

    public float LifeRegenMultiplier { get => 1; }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetHealth(float health)
    {
        this.health = health;
    }

    public void SetWanderingRange(float newRange)
    {
        wanderingRange = newRange;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    private void UpdateVelocity()
    {
        velocityMagnitude = (((Vector2)transform.position - lastPosition) / Time.deltaTime).magnitude;
        lastPosition = transform.position;
    }

    private void IsMovingUpdate()
    {
        noMovementReset = false;

        if (((Vector2)transform.position - noMovementLastPosition).magnitude > 0.3f)
        {
            isMoving = true;
            noMovementCooldown = noMovementMaxCooldown;
            noMovementLastPosition = transform.position;
        }
        else
        {
            noMovementCooldown -= Time.deltaTime;

            if (noMovementCooldown <= 0)
            {
                isMoving = velocityMagnitude > 0.1f;
                noMovementCooldown = noMovementMaxCooldown;
                noMovementReset = true;
            }
        }
    }

    private void MainCooldownsUpdate()
    {
        Timer.ReduceCooldown(ref attackCooldown);
    }

    private bool IsInLineOfSight(Vector2 obj)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, obj - (Vector2)transform.position, (obj - (Vector2)transform.position).magnitude);

        for (int i = 0; i < hits.Length; i++)
        {
            int hitLayer = hits[i].collider.gameObject.layer;
            if (hitLayer == 11 || hitLayer == 0 || hitLayer == 9 || hitLayer == 15) // Map, Default, Hittable, Laser
            {
                bool activeLaser = false;
                if (hitLayer == 15)
                {
                    Laser laser = hits[i].collider.GetComponentInParent<Laser>();

                    if (laser != null)
                    {
                        if (laser.Activated)
                        {
                            activeLaser = true;
                        }
                    }
                }
                if (!hits[i].collider.isTrigger || activeLaser) // Laser exception
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void CheckIsInsideOfWall()
    {
        isInsideOfWall = false;
        isInsideOfSolidWall = false;

        Collider2D target = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Map"));

        if (target != null)
        {
            isInsideOfWall = true;

            if (!target.isTrigger)
            {
                isInsideOfSolidWall = true;
            }
        }
    }

    protected virtual void InsideWallDebugUpdate()
    {
        if (isInsideOfSolidWall)
        {
            // WallEnter
            if (!lastInsideSolidWall || state != EnemyState.None)
            {
                lockStateChange = false;

                AnyStateExit();
                ChangeState(EnemyState.None);

                lockStateChange = true;

                collider.isTrigger = true;
            }

            rb.velocity = ((Vector3)lastValidPosition - transform.position).normalized * 10;
        }
        else
        {
            //WallExit
            if (lastInsideSolidWall)
            {
                lockStateChange = false;

                AnyStateExit();
                ChangeState(EnemyState.Wandering);

                collider.isTrigger = false;
            }

            if (!isInsideOfWall)
            {
                lastValidPosition = transform.position;
            }
        }

        lastInsideSolidWall = isInsideOfSolidWall;
    }

    private void AddHealth(float ammount)
    {
        health += ammount;

        if (health <= 0)
        {
            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyDead);
            Die();
        }
    }

    private Vector2 RandomPoint(float minRange = 1, float maxRange = 2)
    {
        return (Vector2)transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(minRange, maxRange);
    }

    //-----------------------------------------------------------------------------------------------

    protected virtual void OnAliveUpdate() { }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.3f);

        Gizmos.DrawWireSphere(initialPos, wanderingRange);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnNewCollisionEnter2D(collision);
    }

    protected virtual void OnNewCollisionEnter2D(Collision2D collision) { }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnNewCollisionStay2D(collision);
    }

    protected virtual void OnNewCollisionStay2D(Collision2D collision) { }

    private void SetActiveOnDistance(bool value)
    {
        anim.enabled = value;
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            bool newActive = GameManager.IsOnActiveDistance(transform.position, 0, 2f);

            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }

            yield return new WaitForSeconds(1);
        }
    }

    #endregion

}