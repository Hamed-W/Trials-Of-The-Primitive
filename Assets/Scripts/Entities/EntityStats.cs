using UnityEngine;

public class EntityStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int level = 1;

    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseSize = 1f;
    [SerializeField] private float baseMovementSpeed = 1f;

    [SerializeField] private float healthGrowth = 15f;
    [SerializeField] private float damageGrowth = 2f;
    [SerializeField] private float sizeGrowth = 0.02f;
    [SerializeField] private float speedGrowth = 0.05f;

    public float maxHealth;
    public float damage;
    public float size;
    public float movementSpeed;
    public float currentHealth;

    public bool isWeakened = false;

    //public bool blocked = false;



    public void SetLevel(int level)
    {
        this.level = level;
        maxHealth = baseMaxHealth + healthGrowth * (level - 1);
        damage = baseDamage + damageGrowth * (level - 1);
        size = baseSize + sizeGrowth * (level - 1);
        movementSpeed = baseMovementSpeed + speedGrowth * (level - 1);
        currentHealth = maxHealth;
    }

    public bool TakeDamage(float damage)
    {
        if (damage <= 0f)
            return false;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        return currentHealth <= 0f;
    }

    public void Weaken(float weakenAmount)
    {
        if (isWeakened) return;
        Debug.Log("Weakened");
        damage = Mathf.Max(damage - weakenAmount, 0);
        isWeakened = true;
    }
    public void RemoveWeaken()
    {
        if (!isWeakened) return;
        Debug.Log("Removed Weakness");
        damage = baseDamage + damageGrowth * (level - 1);
        isWeakened = false;
    }
}