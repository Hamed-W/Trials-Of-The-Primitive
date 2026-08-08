using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarUI : MonoBehaviour
{
    private const int hotbarSize = 10;

    [SerializeField] private Inventory inventory;

    //public Item selectedItem;
    public int selectedIndex = -1;

    [SerializeField] private HotbarSlotUI[] slots = new HotbarSlotUI[hotbarSize];

    private void OnEnable()
    {
        inventory.InventoryChanged += Refresh;
    }
    private void Start()
    {
        Refresh();
    }


    private void OnDisable()
    {
        inventory.InventoryChanged -= Refresh;
    }

    private void Refresh()
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            Item item = inventory.items[i];

            if (item != null && item.itemData != null)
                slots[i].SetItemSlot(item);
            else
                slots[i].ClearSlot();
        }
    }

    public void OnHotbarSelection(InputValue value)
    {
        int inputNumber = Mathf.RoundToInt(value.Get<float>());
        // Releasing the key sends a value of 0.
        if (inputNumber <= 0) return;
        SelectHotbarSlot(inputNumber - 1);
    }

    private void SelectHotbarSlot(int index)
    {
        if (selectedIndex != -1) slots[selectedIndex].SetSelected(false);
        if (selectedIndex == index)
        {
            selectedIndex = -1;
            inventory.SetItemHeld(null);
            return;
        }
        selectedIndex = index;
        inventory.SetItemHeld(inventory.items[index]);
        slots[index].SetSelected(true);
        Debug.Log($"Selected {index + 1}");
    }
}