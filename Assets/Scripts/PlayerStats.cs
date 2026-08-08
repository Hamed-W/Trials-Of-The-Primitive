using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float baseMaxHealth;
    [SerializeField] private float baseMovementSpeed;
    [SerializeField] private float baseAttackSpeed;
    [SerializeField] private float baseDamageModifier;

    public float maxHealth;
    public float currentHealth;

    public float maxHunger;
    public float currentHunger;
    public float hungerDecreaseRate;

    public float movementSpeed;
    public float attackSpeed;
    public float damageModifier;

    public event Action StatsChanged;
    public event Action PlayerDied; // Set of functions that should happen when player dies.

    private bool dead;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        movementSpeed = baseMovementSpeed;
        attackSpeed = baseAttackSpeed;
        damageModifier = baseDamageModifier;
    }

    private void Update()
    {
        if (dead) return;

        DecreaseHunger();
    }

    private void DecreaseHunger()
    {
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger); // Keeps hunger between 0 and maxHunger.

        StatsChanged?.Invoke();

        if (currentHunger <= 0f) Die();
    }

    public void TakeDamage(float damage)
    {
        if (dead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        StatsChanged?.Invoke();

        if (currentHealth <= 0f) Die();
    }

    public bool Heal(float amount)
    {
        if (dead) return false;

        if (currentHealth >= maxHealth) return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        StatsChanged?.Invoke();

        return true;
    }

    public bool RestoreHunger(float amount)
    {
        if (dead) return false;

        if (currentHunger >= maxHunger) return false;

        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);

        StatsChanged?.Invoke();

        return true;
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        Debug.Log("Player died.");

        PlayerDied?.Invoke();
    }

    public void RecalculateStats(List<Item> equippedItems)
    {
        maxHealth = baseMaxHealth;
        movementSpeed = baseMovementSpeed;
        attackSpeed = baseAttackSpeed;
        damageModifier = baseDamageModifier;

        foreach (Item item in equippedItems)
        {
            if (item == null || item.itemData == null)
                continue;

            foreach (EquipmentStatModifiers modifier in item.itemData.statModifiers)
            {
                switch (modifier.statType)
                {
                    case PlayerStatType.MaxHealth:
                        maxHealth += modifier.amount;
                        break;

                    case PlayerStatType.MovementSpeed:
                        movementSpeed += modifier.amount;
                        break;

                    case PlayerStatType.AttackSpeed:
                        attackSpeed += modifier.amount;
                        break;

                    case PlayerStatType.Damage:
                        damageModifier += modifier.amount;
                        break;
                }
            }
        }

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        StatsChanged?.Invoke();
    }
}

public enum PlayerStatType
{
    MaxHealth,
    MovementSpeed,
    AttackSpeed,
    Damage
}