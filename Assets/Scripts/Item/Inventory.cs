using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;

    public List<Item> items = new List<Item>(); //Use ArrayList because if capacity changes we can use Add() and Remove() to change the size of the list. If we used an array, we would have to create a new array and copy the items over every time the capacity changes.

    public List<Item> equippedItems = new List<Item>();

    public event Action InventoryChanged;

    [SerializeField] private Transform itemDropPoint;


    private void Awake()
    {
        InitialiseSlots();
    }

    private void InitialiseSlots()
    {
        while (items.Count < capacity)
        {
            items.Add(null);
        }

        if (items.Count > capacity) items.RemoveRange(capacity, items.Count - capacity);
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        if (firstIndex == secondIndex)
            return;

        Item temporaryItem = items[firstIndex];

        items[firstIndex] = items[secondIndex];
        items[secondIndex] = temporaryItem;

        InventoryChanged?.Invoke();
    }



    public int AddItem(ItemData itemData, int quantity = 1)
    {
        int startQuantity = quantity;
        if (itemData.stackable) // This fills any existing item stacks first, before creating new stacks.
        {
            foreach (Item item in items)
            {
                if (item.itemData != itemData) continue;

                quantity = item.AddQuantity(quantity); // Returns overflow quantity if the stack is full, hence "quantityRemaining".

                if (quantity <= 0) // Exits early if the entire quantity has been added to existing stacks.
                {
                    InventoryChanged?.Invoke();
                    return quantity;
                }
            }
        }

        // New stack creation if the old stack didn't fill the entire quantity.
        while (quantity > 0)
        {
            int emptyIndex = items.FindIndex(item => item == null); // Finds the first empty slot in the inventory.
            if (emptyIndex == -1) break;

            int stackQuantity = itemData.stackable?
                Mathf.Min(quantity, itemData.maximumStackSize)
                : 1;

            items[emptyIndex] = new Item(itemData, stackQuantity);

            quantity -= stackQuantity;
        }

        if (startQuantity > quantity) InventoryChanged?.Invoke();

        return quantity;
    }

    public bool RemoveItem(ItemData itemData, int quantity = 1)
    {
        int availableQuantity = GetItemQuantity(itemData);
        if (availableQuantity < quantity) return false;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            Item item = items[i];
            if (item == null) continue;
            if (item.itemData != itemData) continue;

            quantity -= item.RemoveQuantity(quantity);
            if (!item.HasQuantity()) items[i] = null;
            if (quantity <= 0) break;
        }

        InventoryChanged?.Invoke();

        return true;
    }

    public int GetItemQuantity(ItemData itemData)
    {
        int total = 0;

        foreach (Item item in items)
        {
            if (item == null) continue;
            if (item.itemData == itemData)
                total += item.quantity;
        }

        return total;
    }

    public Item RemoveItem(int index)
    {
        Item removedItem = items[index];
        if (removedItem == null) return null;
        items[index] = null;

        InventoryChanged?.Invoke();

        return removedItem;
    }

    public bool DropItem(int index)
    {
        Vector3 spawnPosition = itemDropPoint.position;
        Quaternion spawnRotation = itemDropPoint.rotation;

        Item item = items[index];
        if (item == null || item.itemData == null || item.itemData.worldPrefab == null) return false;

        GameObject droppedObject = Instantiate(item.itemData.worldPrefab, spawnPosition, spawnRotation);

        PickUpItem pickup = droppedObject.GetComponent<PickUpItem>();
        if (pickup != null) pickup.SetQuantity(item.quantity);

        items[index] = null;
        InventoryChanged?.Invoke();
        return true;
    }
}