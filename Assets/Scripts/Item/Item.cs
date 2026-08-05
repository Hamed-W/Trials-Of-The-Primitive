using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData itemData;
    public int quantity;

    public Item(ItemData itemData, int quantity = 1)
    {
        this.itemData = itemData;

        this.quantity = Mathf.Clamp(quantity, 1, itemData.maximumStackSize);
    }

    public int AddQuantity(int amount)
    {
        int availableSpace = itemData.maximumStackSize - quantity;

        int amountAdded = Mathf.Min(amount, availableSpace);

        quantity += amountAdded;

        return amount - amountAdded;
    }

    public int RemoveQuantity(int amount)
    {
        int amountRemoved = Mathf.Min(amount, quantity);

        quantity -= amountRemoved;

        return amountRemoved;
    }

    public bool HasQuantity()
    {
        return quantity > 0;
    }
}
