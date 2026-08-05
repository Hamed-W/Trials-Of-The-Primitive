using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform content;
    [SerializeField] private InventorySlot slotPrefab;

    private readonly List<InventorySlot> slots = new();

    private void Awake()
    {
        CreateSlots();
    }

    // Add the Refresh method to the InventoryChanged event when the UI is enabled, so that the UI updates whenever the inventory changes.
    private void OnEnable()
    {
        inventory.InventoryChanged += Refresh;
        Refresh();
    }

    // Remove the Refresh method from the InventoryChanged event when the UI is disabled, to prevent unnecessary updates when the UI is not visible.
    private void OnDisable()
    {
        inventory.InventoryChanged -= Refresh;
    }

    private void CreateSlots()
    {
        for (int i = 0; i < inventory.capacity; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, content);

            slots.Add(slot);
        }
    }

    private void Refresh()
    {
        /*
        if (slots.Count != inventory.capacity)
        {
            Debug.Log("The number of slots does not match the inventory capacity.");
            slots = new();
            CreateSlots();
        }*/

        for (int i = 0; i < slots.Count; i++)
        {
            // inventory.items is a List<Item>, without a predetermined size, so it can have a size of 5, while we loop through each slot which is determined to be inventory.capacity length (i.e. 20).
            // Hence, we need to catch the ArgumentOutOfRangeException, when it happens it means that there's no item in that slot.
            try
            {
                Item item = inventory.items[i];
                if (item != null) slots[i].SetItemSlot(item);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Debug.Log(i);
                slots[i].ClearSlot();
            }
        }
    }
}