using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveEntityBehaviour : EntityBehaviour
{
    [SerializeField] protected int attackType = 0;

    [SerializeField] protected int numberOfAttackTypes = 1;


    [SerializeField] protected float attackCooldownDuration = 2f;

    protected bool attackAnimationFinished = true;

    [SerializeField] protected float baseAttackRange = 2.5f;

    [SerializeField] protected float attackRange;

    [SerializeField] protected List<AttackData> attackData;

    [SerializeField] protected float attackMoveSpeedMultiplier = 0.4f;


    public bool playerAttacked = false;


    public override void SetLevel(int level)
    {
        base.SetLevel(level);
        attackRange = baseAttackRange * entityStats.size;
    }

    protected override EntityState GetTargetReactionState()
    {
        return EntityState.Chasing;
    }

    protected override void UpdateChasing()
    {
        if (target == null)
        {
            ChangeState(EntityState.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            ChangeState(EntityState.Attacking);
            return;
        }

        agent.SetDestination(target.position);
    }

    protected override void UpdateAttacking()
    {
        if (target == null)
        {
            if (attackAnimationFinished) ChangeState(EntityState.Idle);
            return;
        }

        FaceTarget();

        float distance = Vector3.Distance(transform.position, target.position);
        float stopDistance = attackRange * 0.7f;

        if (distance <= stopDistance)
        {
            agent.isStopped = true;
            animator.SetBool("Idle", true);
        }
        else
        {
            agent.isStopped = false;
            animator.SetBool("Idle", false);
            agent.SetDestination(target.position);
        }

        if (!attackAnimationFinished)
        {
            agent.speed = runningSpeed * attackMoveSpeedMultiplier;
            return;
        }

        agent.speed = runningSpeed;

        if (distance > attackRange)
        {
            ChangeState(EntityState.Chasing);
            return;
        }

        if (attackCooldownTimer <= 0f)
        {
            StartAttack();
        }
    }
    private void StartAttack()
    {
        attackAnimationFinished = false;

        agent.speed = runningSpeed * attackMoveSpeedMultiplier;

        PrepareAttack();

        animator.SetTrigger(Animator.StringToHash("Attack"));
    }

    private void PrepareAttack()
    {
        attackType = Random.Range(0, numberOfAttackTypes);
        animator.SetInteger(Animator.StringToHash("Attack Type"), attackType);
    }


    public void OnAttackAnimationFinished()
    {
        attackAnimationFinished = true;

        attackCooldownTimer = attackCooldownDuration;
    }
    
    public void OnAttackWindowEntry()
    {
        for (int i = 0; i < attackData[attackType].colliders.Length; i++)
        {
            attackData[attackType].colliders[i].ActivateCollider();
        }
        playerAttacked = false;
    }

    public void OnAttackWindowExit()
    {
        for (int i = 0; i < attackData[attackType].colliders.Length; i++)
        {
            attackData[attackType].colliders[i].DeactivateCollider();
        }
    }

    public void OnAttackHit(GameObject player)
    {
        if (playerAttacked == false)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            playerStats.TakeDamage(entityStats.damage);

            Debug.Log($"Player took {entityStats.damage} damage from {this.name}");
            playerAttacked = true;
        }
    }
}

[System.Serializable]
public class AttackData
{
    //public float damage;
    //public float range;
    //public float cooldown;
    public AttackColliderHandler[] colliders;
}