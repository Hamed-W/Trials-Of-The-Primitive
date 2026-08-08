using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform content;
    [SerializeField] private InventorySlot slotPrefab;

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private GameObject splitStackPanel;

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

            slot.Initialise(inventory, i, rootCanvas, GetComponent<RectTransform>(), splitStackPanel);

            slots.Add(slot);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Item item = inventory.items[i];

            if (item != null && item.itemData != null)
                slots[i].SetItemSlot(item);
            else
                slots[i].ClearSlot();
        }
    }
}