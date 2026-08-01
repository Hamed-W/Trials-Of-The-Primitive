using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Search.SearchColumn;

public class EntityStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int level = 1;

    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseSize = 1f;
    [SerializeField] private float baseMovementSpeed = 3.5f;

    [SerializeField] private float healthGrowth = 15f;
    [SerializeField] private float damageGrowth = 2f;
    [SerializeField] private float sizeGrowth = 0.02f;
    [SerializeField] private float speedGrowth = 0.05f;

    [SerializeField] private float maxHealth;
    [SerializeField] private float damage;
    [SerializeField] private float size;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float currentHealth;



    public void SetLevel(int level)
    {
        this.level = level;
        maxHealth = baseMaxHealth + healthGrowth * (level - 1);
        damage = baseDamage + damageGrowth * (level - 1);
        size = baseSize + sizeGrowth * (level - 1);
        movementSpeed = baseMovementSpeed + speedGrowth * (level - 1);
        currentHealth = maxHealth;
    }
}