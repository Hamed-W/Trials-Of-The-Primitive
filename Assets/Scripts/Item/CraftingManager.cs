using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    public List<Item> craftingItems = new List<Item>();

    public event Action CraftingChanged;

    [SerializeField] private Transform itemDropPoint;

    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    private void Awake()
    {
        InitialiseSlots();
    }

    private void InitialiseSlots()
    {
        while (craftingItems.Count < 9)
        {
            craftingItems.Add(null);
        }
    }

    public CraftingRecipe FindMatchingRecipe()
    {
        foreach (CraftingRecipe recipe in recipes)
        {
            if (IsCorrectRecipe(recipe)) return recipe;
        }

        return null;
    }

    private bool IsCorrectRecipe(CraftingRecipe recipe)
    {
        for (int i = 0; i < 9; i++)
        {
            RecipeIngredient required = recipe.ingredients[i];

            Item actual = craftingItems[i];

            // Recipe expects this slot to be empty.
            if (required == null || required.itemData == null)
            {
                if (actual != null)
                {
                    if (actual.itemData != null)
                        return false;
                }

                continue;
            }

            // Recipe expects an item but slot is empty.
            if (actual == null || actual.itemData == null)
            {
                return false;
            }

            // Wrong item.
            if (required.itemData != actual.itemData)
                return false;

            // Not enough quantity.
            if (required.quantity > actual.quantity)
                return false;
        }

        return true;
    }

    public void Craft()
    {
        CraftingRecipe recipe = FindMatchingRecipe();

        if (recipe == null)
        {
            Debug.Log("No matching recipe.");
            return;
        }

        CraftRecipe(recipe);
    }

    // Removes the required quantity for each slot in the 3 by 3 crafting grid. It then adds it to the inventory.
    private void CraftRecipe(CraftingRecipe recipe)
    {
        for (int i = 0; i < 9; i++)
        {
            RecipeIngredient required = recipe.ingredients[i];

            if (required == null || required.itemData == null) continue;

            Item craftingItem = craftingItems[i];

            craftingItem.RemoveQuantity(required.quantity);

            if (!craftingItem.HasQuantity()) craftingItems[i] = null;
        }

        // Remaining is how many is left after trying to fit the result into the inventory.
        int remaining = inventory.AddItem(recipe.resultItem, recipe.resultQuantity);

        // It couldn't all fit, so we instantiate it into the ground.
        if (remaining > 0)
        {
            GameObject droppedObject = Instantiate(recipe.resultItem.worldPrefab, itemDropPoint.position, itemDropPoint.rotation);
            PickUpItem pickup = droppedObject.GetComponent<PickUpItem>();
            if (pickup != null) pickup.SetQuantity(remaining);
        }

        CraftingChanged?.Invoke();
    }


    public Item GetItem(int index)
    {
        return craftingItems[index];
    }

    // Swaps items between crafting indexes.
    public void SwapItems(int firstIndex, int secondIndex)
    {
        Item temp = craftingItems[firstIndex];

        craftingItems[firstIndex] = craftingItems[secondIndex];

        craftingItems[secondIndex] = temp;

        CraftingChanged?.Invoke();
    }

    // Swaps items between crafting grid and inventory grid.
    public void SwapInventoryAndCrafting(int inventoryIndex, int craftingIndex)
    {
        Item inventoryItem = inventory.items[inventoryIndex];
        Item craftingItem = craftingItems[craftingIndex];

        if ((craftingItem == null || craftingItem.itemData == null) && (inventoryItem == null || inventoryItem.itemData == null))
            return;

        inventory.RemoveItem(inventoryIndex);

        craftingItems[craftingIndex] = inventoryItem;

        if (craftingItem != null)
        {
            inventory.AddItem(craftingItem.itemData, craftingItem.quantity);
        }

        CraftingChanged?.Invoke();
    }

    // Instantiates the world prefab of an item and spawns it in front of hte player (itemDropPoint).
    public bool DropItem(int index)
    {
        Vector3 spawnPosition = itemDropPoint.position;
        Quaternion spawnRotation = itemDropPoint.rotation;

        Item item = craftingItems[index];
        if (item == null || item.itemData == null || item.itemData.worldPrefab == null) return false;

        GameObject droppedObject = Instantiate(item.itemData.worldPrefab, spawnPosition, spawnRotation);

        PickUpItem pickup = droppedObject.GetComponent<PickUpItem>();
        if (pickup != null) pickup.SetQuantity(item.quantity);

        craftingItems[index] = null;
        CraftingChanged?.Invoke();
        return true;
    }

    // Drops all remaining items in Crafting grid (for when inventory is closed).
    public void ClearCraftingItems()
    {
        for (int i = 0; i < craftingItems.Count; i++)
        {
            Item item = craftingItems[i];

            if (item == null)
                continue;

            if (item.itemData == null)
            {
                craftingItems[i] = null;
                continue;
            }

            item.quantity = inventory.AddItem(item.itemData, item.quantity);

            if (item.quantity > 0)
            {
                DropItem(i);
            }
            else
            {
                craftingItems[i] = null;
            }
        }

        CraftingChanged?.Invoke();
    }
}