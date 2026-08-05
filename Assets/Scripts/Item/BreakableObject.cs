using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 30f;

    private float currentHealth;
    private bool destroyed; //Allows me to later invoke the Destroy function (delay it for some animation) without it being called multiple times. Also if somehow player calls TakeDamage after the object is destroyed, it won't call DestroyObject again.

    [SerializeField] private ItemDropper itemDropper;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    void Start()
    {
        DestroyObject(); // For testing purposes, destroy the object immediately
    }

    public void TakeDamage(float damage)
    {
        if (destroyed) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
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