using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveEntityBehaviour : EntityBehaviour
{
    [SerializeField] private float damage = 25f;

    [SerializeField] private int attackType = 0;

    [SerializeField] private int numberOfAttackTypes = 1;


    [SerializeField] protected float attackCooldownDuration = 2f;

    protected bool attackAnimationFinished = true;

    [SerializeField] protected float attackRange = 2f;



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

        Debug.Log(attackAnimationFinished + "Attack animation finished");
        if (!attackAnimationFinished) return;

        if (distance > attackRange)
        {
            animator.SetBool("Idle", false);
            Debug.Log($"Leaving attack range: {distance}");
            ChangeState(EntityState.Chasing);
            return;
        }
        else
        {
            animator.SetBool("Idle", true);
        }

        if (attackCooldownTimer <= 0f)
        {
            Debug.Log("Starting follow-up attack");
            StartAttack();
        }
    }
    private void StartAttack()
    {
        Debug.Log("Set to false!");
        attackAnimationFinished = false;
        //attackCooldownTimer = attackCooldownDuration;

        PrepareAttack();
        animator.SetTrigger(Animator.StringToHash("Attack"));
    }

    private void PrepareAttack()
    {
        attackType = Random.Range(0, numberOfAttackTypes);
        animator.SetInteger(Animator.StringToHash("Attack Type"), attackType);

        Debug.Log($"Selected attack type: {attackType}");
    }


    public void OnAttackAnimationFinished()
    {
        Debug.Log("Event called");
        attackAnimationFinished = true;
        attackCooldownTimer = attackCooldownDuration;
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