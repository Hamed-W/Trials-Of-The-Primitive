using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemUseController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float swingCooldown = 2f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float useItemCooldown = 1f;

    private bool canSwing = true;
    private bool canUseItem = true;
    public bool isBlocking = false;


    public SwingableCollision swingableCollision;
    public float itemAttackSpeed;

    public ShieldBlock shieldBlock;

    [SerializeField] private PlayerStats playerStats;

    void Awake()
    {
        playerStats.StatsChanged += UpdateAttackSpeed;
    }

    private void UpdateAttackSpeed()
    {
        attackSpeed = playerStats.attackSpeed;
    }

    //Animation events for setting swing hit window.
    public void BeginSwingHitWindow()
    {
        if (swingableCollision == null) return;
        swingableCollision.StartSwing();
        StopBlocking();
    }

    public void EndSwingHitWindow()
    {
        if (swingableCollision == null) return;
        swingableCollision.EndSwing();
    }

    public bool SwingItem()
    {
        if (!canSwing) return false;

        canSwing = false;

        float finalAttackSpeed = attackSpeed * itemAttackSpeed;

        animator.SetFloat("attackSpeed", attackSpeed);
        animator.SetTrigger("swing");

        Invoke(nameof(ResetSwing), swingCooldown / finalAttackSpeed);
        return true;
    }

    public bool UseItem()
    {
        if (!canUseItem) return false;
        canUseItem = false;

        //animator.SetTrigger("useItem");
        
        Invoke(nameof(ResetUseItem), useItemCooldown);
        return true;
    }

    public bool UseConsumableItem()
    {
        if (!canUseItem) return false;
        canUseItem = false;
        animator.SetTrigger("consumeItem");

        Invoke(nameof(ResetUseItem), useItemCooldown);
        return true;
    }

    public void StartBlocking()
    {
        if (swingableCollision == null || swingableCollision.midSwing != true)
        { 
            animator.SetBool("isBlocking", true);
            isBlocking = true;
        }
    }

    public void StopBlocking()
    {
        animator.SetBool("isBlocking", false);
        isBlocking = false;
    }


    private void ResetUseItem()
    {
        canUseItem = true;
    }

    private void ResetSwing()
    {
        canSwing = true;

        if (swingableCollision != null) {
            swingableCollision.ResetHit();
        }
    }
}
