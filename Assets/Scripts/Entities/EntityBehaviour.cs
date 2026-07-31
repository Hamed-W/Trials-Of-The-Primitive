using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EntityBehaviour : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;

    [SerializeField] protected float detectionRange = 12f;
    [SerializeField] protected float idleDuration = 3f;

    [SerializeField] protected float roamingRadius = 10f;
    [SerializeField] protected float roamingSpeed = 1.5f;
    [SerializeField] protected float runningSpeed = 3.5f;
    [SerializeField] protected float turnSpeed = 360f;

    [SerializeField] protected float maximumDistanceFromSpawn = 25f;
    [SerializeField] protected float returnStoppingDistance = 1.5f;

    protected float attackCooldownTimer;

    protected Transform target;
    protected EntityState currentState;

    private float timer;
    private Vector3 spawnPosition;

    protected virtual void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        spawnPosition = transform.position;
    }

    protected virtual void Start()
    {
        EnterState(EntityState.Idle);
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
        Vector3 randomOffset = Random.insideUnitSphere * roamingRadius;

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

    public virtual void Die()
    {
        ChangeState(EntityState.Dead);
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