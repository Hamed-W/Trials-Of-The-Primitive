using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxBehaviour : EntityBehaviour
{
    [SerializeField] private float fleeDistance = 15f;

    protected override EntityState GetTargetReactionState()
    {
        return EntityState.Fleeing;
    }

    protected override void UpdateFleeing()
    {
        if (target == null)
        {
            ChangeState(EntityState.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position,target.position);

        if (distance >= fleeDistance)
        {
            target = null;
            ChangeState(EntityState.Idle);
            return;
        }

        Vector3 directionAway = (transform.position - target.position).normalized;

        Vector3 desiredPosition = transform.position + directionAway * roamingRadius;

        if (UnityEngine.AI.NavMesh.SamplePosition(desiredPosition, out UnityEngine.AI.NavMeshHit hit, roamingRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}