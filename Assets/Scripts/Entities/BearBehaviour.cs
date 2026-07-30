using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearBehaviour : EntityBehaviour
{
    [SerializeField] private float damage = 25f;

    [SerializeField] private int attackType = 0;



    protected override EntityState GetTargetReactionState()
    {
        return EntityState.Chasing;
    }

    protected override void ExitState(EntityState state)
    {
        base.ExitState(state);
    }

    protected override void PrepareAttack()
    {
        attackType = Random.Range(0, 4);
        animator.SetInteger(Animator.StringToHash("Attack Type"), attackType);

        Debug.Log($"Selected attack type: {attackType}");
    }
    public void ApplyAttackDamage()
    {
        if (target == null)
            return;

        float distance =
            Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
            return;

        // Replace with health functionality
        // target.GetComponent<HealthScript>()?.TakeDamage(damage); or something like this
    }
}