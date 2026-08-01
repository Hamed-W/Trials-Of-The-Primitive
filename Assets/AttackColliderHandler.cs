using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackColliderHandler : MonoBehaviour
{
    [SerializeField] private Collider attackCollider;
    //public bool attacked;
    [SerializeField] private AggressiveEntityBehaviour behaviourScript;

    public void DeactivateCollider()
    {
        attackCollider.enabled = false;
    }
    public void ActivateCollider()
    {
        attackCollider.enabled = true;
    }
    /*
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !behaviourScript.playerAttacked)
        {
            Debug.Log("Player is in attack range");
            behaviourScript.OnAttackHit(other.gameObject);
            //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        }
    }*/
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !behaviourScript.playerAttacked)
        {
            Debug.Log("Player is in attack range");
            behaviourScript.OnAttackHit(other.gameObject);
            //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        }
    }
}
