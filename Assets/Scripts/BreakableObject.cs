using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 20f;

    [SerializeField] private float currentHealth;
    private bool destroyed; //Allows me to later invoke the Destroy function (delay it for some animation) without it being called multiple times. Also if somehow player calls TakeDamage after the object is destroyed, it won't call DestroyObject again.

    [SerializeField] private ItemDropper itemDropper;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Hi");
        if (damage < (0.1 * maximumHealth)) return;
        Debug.Log($"Took {damage} damage");
        if (destroyed) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            BiomeObjectRespawn respawn = GetComponent<BiomeObjectRespawn>();
            if (respawn != null) respawn.Respawn();
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        destroyed = true;
        itemDropper.DropItems();
        Destroy(gameObject);
    }
}