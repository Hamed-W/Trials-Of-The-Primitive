using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;

    public List<Item> items = new List<Item>(); //Use ArrayList because if capacity changes we can use Add() and Remove() to change the size of the list. If we used an array, we would have to create a new array and copy the items over every time the capacity changes.

    public List<Item> equippedItems = new List<Item>();
    public Item itemHeld = null; //Separate from equippedItems because you can hold an item in your hand without equipping it, and you can equip an item without holding it in your hand.
    private GameObject heldItemObject;

    [SerializeField] private List<BoneAttachmentEntry> boneAttachments = new List<BoneAttachmentEntry>();

    [Serializable]
    public class BoneAttachmentEntry
    {
        public EquipmentAttachment attachment;
        public Transform attachmentPoint;
    }

    public event Action InventoryChanged;

    [SerializeField] private Transform itemDropPoint;

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private ItemUseController itemUseController;

    public bool splitStack = false;

    [SerializeField] private GameObject bodyArmor;

    [SerializeField] private List<ItemUseType> equipmentValidationTypes = new List<ItemUseType>();


    public bool inputEnabled = true;



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
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        if (firstIndex == secondIndex)
            return;

        if (itemHeld == items[firstIndex])
        {
            SetItemHeld(items[secondIndex]);
        }
        else if (itemHeld == items[secondIndex])
        {
            SetItemHeld(items[firstIndex]);
        }

        Item temporaryItem = items[firstIndex];

        items[firstIndex] = items[secondIndex];
        items[secondIndex] = temporaryItem;


        InventoryChanged?.Invoke();
    }



    public int AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null)
        {
            Debug.LogError("Inventory.AddItem was called with NULL ItemData!");
            return quantity;
        }

        int startQuantity = quantity;
        if (itemData.maximumStackSize > 1) // This fills any existing item stacks first, before creating new stacks.
        {
            foreach (Item item in items)
            {
                if (item == null || item.itemData == null)
                    continue;

                if (item.itemData != itemData)
                    continue;

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
            int emptyIndex = items.FindIndex(item => item == null || item.itemData == null); // Finds the first empty slot in the inventory.
            if (emptyIndex == -1) break;


            int stackQuantity = Mathf.Min(quantity, itemData.maximumStackSize);

            items[emptyIndex] = new Item(itemData, stackQuantity);

            quantity -= stackQuantity;
        }

        if (startQuantity > quantity) InventoryChanged?.Invoke();

        return quantity;
    }

    public bool RemoveItemQuantity(ItemData itemData, int quantity = 1)
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

        if (itemHeld == items[index]) SetItemHeld(null);

        if (equippedItems.Contains(removedItem) && GetItemQuantity(removedItem.itemData) <= 0)
            UnequipItem(removedItem);

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

        if (itemHeld == item) SetItemHeld(null);
        if (equippedItems.Contains(item)) UnequipItem(item);

        GameObject droppedObject = Instantiate(item.itemData.worldPrefab, spawnPosition, spawnRotation);

        PickUpItem pickup = droppedObject.GetComponent<PickUpItem>();
        if (pickup != null) pickup.SetQuantity(item.quantity);

        items[index] = null;
        InventoryChanged?.Invoke();
        return true;
    }

    public void EquipItem(Item item)
    {
        if (item == null || item.itemData == null) return;
        if (!ValidateEquipment(item)) return;
        equippedItems.Add(item);
        if (item.itemData.attachment != EquipmentAttachment.None)
        {
            Transform attachmentPoint = boneAttachments.Find(entry => entry.attachment == item.itemData.attachment).attachmentPoint;
            GameObject equippedObject = Instantiate(item.itemData.equippedPrefab, attachmentPoint);
            if (item.itemData.itemUseType == ItemUseType.Shield)
            {
                itemUseController.shieldBlock = equippedObject.GetComponentInChildren<ShieldBlock>();
                if (itemUseController.shieldBlock != null)
                {
                    itemUseController.shieldBlock.itemData = item.itemData;
                    itemUseController.shieldBlock.playerStats = playerStats;
                }
            }
        }
        if (item.itemData.itemUseType == ItemUseType.Armor)
        {
            GameObject armorChildInPrefab = item.itemData.inHandPrefab.transform.Find("Body Armor").gameObject;
            Renderer prefabRenderer = armorChildInPrefab.GetComponent<Renderer>();
            Renderer liveArmorRenderer = bodyArmor.GetComponent<Renderer>();
            liveArmorRenderer.material = prefabRenderer.sharedMaterial;
            bodyArmor.SetActive(true);
        }

        InventoryChanged?.Invoke();
        playerStats.RecalculateStats(equippedItems);
    }

    private bool ValidateEquipment(Item itemToEquip)
    {
        ItemUseType itemUseType = itemToEquip.itemData.itemUseType;

        if (equipmentValidationTypes.Contains(itemUseType))
        {
            Item equippedItem = equippedItems.Find(e => e.itemData.itemUseType == itemUseType);
            if (equippedItem != null)
            {
                if (itemUseType == ItemUseType.Shield) itemUseController.StopBlocking();
                UnequipItem(equippedItem);
                if (equippedItem.itemData != itemToEquip.itemData) return true;
                return false;
            }
        }
        return true;
    }

    public void UnequipItem(Item item)
    {
        equippedItems.Remove(item);
        if (item.itemData.attachment != EquipmentAttachment.None)
        {
            Transform attachmentPoint = boneAttachments.Find(entry => entry.attachment == item.itemData.attachment).attachmentPoint;
            foreach (Transform child in attachmentPoint)
            {
                if (child.name.Equals(item.itemData.equippedPrefab.name + "(Clone)"))
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
        if (item.itemData.itemUseType == ItemUseType.Armor)
        {
            bodyArmor.SetActive(false);
        }
        if (item.itemData.itemUseType == ItemUseType.Shield) itemUseController.swingableCollision = null;
        InventoryChanged?.Invoke();
        playerStats.RecalculateStats(equippedItems);
    }

    public void OnUseItem(InputValue value)
    {
        if (!inputEnabled) return;

        if (itemHeld == null || itemHeld.itemData == null)
        {
            bool used = itemUseController.SwingHand();
            return;
        }

        if (itemHeld.itemData.equippable)
        {
            EquipItem(itemHeld);
            return;
        }
        else
        {
            if (itemHeld.itemData.itemUseType == ItemUseType.Use || itemHeld.itemData.itemUseType == ItemUseType.Consume)
            {
                bool used = (itemHeld.itemData.itemUseType == ItemUseType.Consume) ? itemUseController.UseConsumableItem() : itemUseController.UseItem();
                if (!used) return;
                used = itemHeld.itemData.useEffect.Use(new ItemUseContext(player, this, itemHeld));
                if (used) itemHeld.RemoveQuantity(1);

                // If the item stack has no quantity, then remove the item stack from inventory and set the held item to null.
                if (!itemHeld.HasQuantity())
                {
                    RemoveItem(items.IndexOf(itemHeld));
                    //SetItemHeld(null);
                }
            }
            else if (itemHeld.itemData.itemUseType == ItemUseType.Swing)
            {
                bool used = itemUseController.SwingItem();
                if (!used) return;
            }
        }
        InventoryChanged.Invoke();
    }

    public void OnUseShield(InputValue value)
    {
        if (equippedItems.Find(e => e.itemData.itemUseType == ItemUseType.Shield) == null) return;
        if (!value.isPressed)
        {
            itemUseController.StopBlocking();
            return;
        }
        itemUseController.StartBlocking();
    }

    public void SetItemHeld(Item item)
    {
        // Remove the gameobject belonging to the previously held item.
        if (heldItemObject != null)
        {
            itemUseController.swingableCollision = null;

            Destroy(heldItemObject);
            heldItemObject = null;
        }

        itemHeld = item;

        // Selecting an empty hotbar slot simply leaves the hand empty.
        if (itemHeld == null || itemHeld.itemData == null || itemHeld.itemData.inHandPrefab == null) return;

        BoneAttachmentEntry rightHandEntry = boneAttachments.Find(entry => entry.attachment == EquipmentAttachment.RightHand);


        heldItemObject = Instantiate(itemHeld.itemData.inHandPrefab, rightHandEntry.attachmentPoint);
        //Set swingable collision if possible to reset the hit state when the item is used. This is for pickaxe particle effects.
        itemUseController.swingableCollision = heldItemObject.GetComponentInChildren<SwingableCollision>();
        if (itemUseController.swingableCollision != null)
        {
            itemUseController.swingableCollision.itemData = item.itemData;
            itemUseController.swingableCollision.playerStats = playerStats;
            itemUseController.itemAttackSpeed = item.itemData.statModifiers.Find(e => e.statType == PlayerStatType.AttackSpeed).amount;
        }
    }

    public void SplitItemStack(Item item, int newAmount, int index)
    {
        item.quantity -= newAmount;
        if (!item.HasQuantity()) RemoveItem(index);
        Item newItem = new Item(item.itemData, newAmount);
        int emptyIndex = items.FindIndex(item => item == null || item.itemData == null); // Finds the first empty slot in the inventory.
        if (emptyIndex == -1)
        {
            GameObject droppedObject = Instantiate(newItem.itemData.worldPrefab, itemDropPoint.position, itemDropPoint.rotation);
            PickUpItem pickup = droppedObject.GetComponent<PickUpItem>();
            if (pickup != null) pickup.SetQuantity(newAmount);
            return;
        }
        items[emptyIndex] = newItem;
        InventoryChanged?.Invoke();
    }

    public void RefreshInventory()
    {
        InventoryChanged?.Invoke();
    }

}