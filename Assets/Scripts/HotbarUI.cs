using System.Collections;
using System.Collections.Generic;
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
            try
            {
                Item item = inventory.items[i];
                slots[i].SetItemSlot(item);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                slots[i].ClearSlot();
            }
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
            return;
        }
        selectedIndex = index;
        //try selectedItem = inventory.items[index];
        slots[index].SetSelected(true);
        Debug.Log($"Selected {index + 1}");
    }
}