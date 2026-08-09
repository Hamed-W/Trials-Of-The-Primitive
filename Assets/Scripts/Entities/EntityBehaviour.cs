using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using System;

public abstract class EntityBehaviour : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform model;

    [SerializeField] protected float detectionRange = 12f;
    [SerializeField] protected float idleDuration = 3f;
    
    [SerializeField] protected float baseRoamingRadius = 10f;
    [SerializeField] protected float baseRoamingSpeed = 1.5f;
    [SerializeField] protected float baseRunningSpeed = 3.5f;
    [SerializeField] protected float baseTurnSpeed = 360f;

    [SerializeField] protected float roamingRadius;
    [SerializeField] protected float roamingSpeed;
    [SerializeField] protected float runningSpeed;
    [SerializeField] protected float turnSpeed;
    private Vector3 originalModelScale;

    /*
    [SerializeField] protected float roamingRadius => entityStats.size * 10f;
    [SerializeField] protected float roamingSpeed => entityStats.size * 1.5f;
    [SerializeField] protected float runningSpeed => entityStats.movementSpeed * 3.5f;
    [SerializeField] protected float turnSpeed => entityStats.movementSpeed * 360f;*/

    [SerializeField] protected float maximumDistanceFromSpawn = 25f;
    [SerializeField] protected float returnStoppingDistance = 1.5f;

    protected float attackCooldownTimer;

    protected Transform target;
    protected EntityState currentState;

    private float timer;
    private Vector3 spawnPosition;

    [SerializeField] protected EntityStats entityStats;

    public event Action OnDeath;



    protected virtual void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        originalModelScale = model.localScale;

        spawnPosition = transform.position;

        OnDeath += GetComponent<ItemDropper>().DropItems;
    }

    protected virtual void Start()
    {
        //model.localScale *= entityStats.size;

        //attackCooldownTimer /= attackspeed //For future implementation of attack speed, if needed
        SetLevel(1);
        EnterState(EntityState.Idle);
    }

    public virtual void SetLevel(int level)
    {
        entityStats.SetLevel(level);
        roamingRadius = entityStats.size * baseRoamingRadius;
        roamingSpeed = entityStats.movementSpeed * baseRoamingSpeed;
        runningSpeed = entityStats.movementSpeed * baseRunningSpeed;
        turnSpeed = entityStats.movementSpeed * baseTurnSpeed;
        model.localScale = originalModelScale * entityStats.size;
    }

    protected virtual void Update()
    {
        if (currentState == EntityState.Dead) return;

        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;

        FindTarget();

        if (CanReturnToSpawn() && target == null)
        {
            ChangeState(EntityState.Returning);
        }

        switch (currentState)
        {
            case EntityState.Idle:
                UpdateIdle();
                break;

            case EntityState.Roaming:
                UpdateRoaming();
                break;

            case EntityState.Chasing:
                UpdateChasing();
                break;

            case EntityState.Attacking:
                UpdateAttacking();
                break;

            case EntityState.Fleeing:
                UpdateFleeing();
                break;

            case EntityState.Returning:
                UpdateReturning();
                break;
        }

        UpdateAnimation();
    }

    protected virtual void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            target = null;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);

        target = distance <= detectionRange ? player.transform : null;
    }

    protected virtual void UpdateIdle()
    {
        timer -= Time.deltaTime;

        if (ShouldReactToTarget())
        {
            ChangeState(GetTargetReactionState());
            return;
        }

        if (timer <= 0f)
            ChangeState(EntityState.Roaming);
    }

    protected virtual void UpdateRoaming()
    {
        if (ShouldReactToTarget())
        {
            ChangeState(GetTargetReactionState());
            return;
        }


        if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance))
        {
            ChangeState(EntityState.Idle);
        }
    }

    protected virtual void UpdateChasing()
    {
    }

    protected virtual void UpdateAttacking()
    {
    }

    protected virtual void UpdateFleeing()
    {
    }

    protected virtual void UpdateReturning()
    {
        if (agent.pathPending)
            return;

        if (ShouldReactToTarget())
        {
            ChangeState(GetTargetReactionState());
            return;
        }

        if (agent.remainingDistance <= returnStoppingDistance)
        {
            ChangeState(EntityState.Idle);
        }
    }

    protected virtual bool ShouldReactToTarget()
    {
        return target != null;
    }

    protected abstract EntityState GetTargetReactionState();

    protected virtual void ChangeState(EntityState newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);

        currentState = newState;

        EnterState(newState);
    }

    protected virtual void EnterState(EntityState state)
    {
        switch (state)
        {
            case EntityState.Idle:
                agent.isStopped = true;
                timer = idleDuration;
                agent.speed = roamingSpeed;
                animator.SetBool("Idle", true);
                break;

            case EntityState.Roaming:
                agent.isStopped = false;
                SetRandomRoamingDestination();
                agent.speed = roamingSpeed;
                break;

            case EntityState.Chasing:
                agent.isStopped = false;
                agent.speed = runningSpeed;
                break;

            case EntityState.Attacking:
                agent.isStopped = true;
                break;

            case EntityState.Fleeing:
                agent.isStopped = false;
                agent.speed = runningSpeed;
                break;

            case EntityState.Dead:
                agent.isStopped = true;
                animator.SetTrigger("Death");
                animator.SetBool("DeathBool", true);
                break;

            case EntityState.Returning:
                target = null;

                agent.speed = roamingSpeed;
                agent.isStopped = false;
                agent.SetDestination(spawnPosition);
                break;
        }
    }

    protected virtual void ExitState(EntityState state)
    {
        animator.SetBool(Animator.StringToHash("Idle"), false);
    }


    protected virtual void SetRandomRoamingDestination()
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * roamingRadius;

        randomOffset.y = 0f; // Keep the offset on the horizontal plane of the Navmesh.

        Vector3 candidatePosition = spawnPosition + randomOffset;

        if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, roamingRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected virtual bool CanReturnToSpawn()
    {
        if (currentState == EntityState.Returning)
            return false;

        float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);

        return (distanceFromSpawn > maximumDistanceFromSpawn);
    }

    protected virtual void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),8f * Time.deltaTime); //Smoother turning using Slerp instead of RotateTowards
        //transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),turnSpeed * Time.deltaTime);
    }

    protected virtual void UpdateAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        //I will add HP here after as well for death animation
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == EntityState.Dead) return;
        bool died = entityStats.TakeDamage(damage);
        if (died) Die();
    }
    public virtual void Die()
    {
        ChangeState(EntityState.Dead);
        Invoke(nameof(DestroyEntity), 2f);
    }

    public virtual void DestroyEntity()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }

}

// State machine for entity behavior
public enum EntityState
{
    Idle,
    Roaming,
    Chasing,
    Attacking,
    Fleeing,
    Returning,
    Dead
}