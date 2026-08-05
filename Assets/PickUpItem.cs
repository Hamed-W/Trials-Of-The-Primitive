using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Pass1");
        Inventory inventory = other.GetComponentInChildren<Inventory>();

        quantity = inventory.AddItem(itemData, quantity);

        Debug.Log("Pass2");
        if (quantity > 0)
            return;

        Destroy(gameObject);
    }
}
