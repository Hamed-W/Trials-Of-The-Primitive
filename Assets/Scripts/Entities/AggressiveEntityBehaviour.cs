using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveEntityBehaviour : EntityBehaviour
{
    [SerializeField] protected int attackType = 0;

    [SerializeField] protected int numberOfAttackTypes = 1;


    [SerializeField] protected float attackCooldownDuration = 2f;

    protected bool attackAnimationFinished = true;

    [SerializeField] protected float baseAttackRange = 2f;

    [SerializeField] protected float attackRange;

    [SerializeField] protected List<AttackData> attackData;


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
            if (attackAnimationFinished)
                ChangeState(EntityState.Idle);

            return;
        }

        FaceTarget();

        float distance = Vector3.Distance(transform.position, target.position);

        if (!attackAnimationFinished) return;

        if (distance > attackRange)
        {
            animator.SetBool("Idle", false);
            ChangeState(EntityState.Chasing);
            return;
        }
        else
        {
            animator.SetBool("Idle", true);
        }

        if (attackCooldownTimer <= 0f)
        {
            StartAttack();
        }
    }
    private void StartAttack()
    {
        attackAnimationFinished = false;
        //attackCooldownTimer = attackCooldownDuration;

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

    /*
    public void ApplyAttackDamage()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
            return;

        // Replace with health functionality
        // target.GetComponent<HealthScript>()?.TakeDamage(damage); or something like this
    }*/
}

[System.Serializable]
public class AttackData
{
    //public float damage;
    //public float range;
    //public float cooldown;
    public AttackColliderHandler[] colliders;
}