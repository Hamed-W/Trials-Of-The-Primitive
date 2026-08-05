using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;

    public List<Item> items = new List<Item>();

    public event Action InventoryChanged;

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
        while (quantity > 0 && items.Count < capacity)
        {
            int stackQuantity = itemData.stackable?
                Mathf.Min(quantity, itemData.maximumStackSize)
                : 1;

            items.Add(new Item(itemData, stackQuantity));

            quantity -= stackQuantity;
        }

        if (startQuantity > quantity) InventoryChanged?.Invoke();

        return quantity;
    }

    public bool RemoveItem(ItemData itemData, int quantity = 1)
    {
        int availableQuantity = GetItemQuantity(itemData);

        if (availableQuantity < quantity)
            return false;

        for (int i= items.Count - 1; i >= 0; i--)
        {
            Item item = items[i];

            if (item.itemData != itemData)
                continue;

            quantity -= item.RemoveQuantity(quantity);


            if (!item.HasQuantity())
                items.RemoveAt(i);

            if (quantity <= 0)
                break;
        }

        InventoryChanged?.Invoke();
        return true;
    }

    // Useful for crafting later.
    public int GetItemQuantity(ItemData itemData)
    {
        int total = 0;

        foreach (Item item in items)
        {
            if (item.itemData == itemData)
                total += item.quantity;
        }

        return total;
    }
}