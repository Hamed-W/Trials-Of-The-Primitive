using System.Collections.Generic;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private List<CraftingSlot> slots;

    private void OnEnable()
    {
        craftingManager.CraftingChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        craftingManager.CraftingChanged -= Refresh;
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Item item = craftingManager.craftingItems[i];

            if (item != null && item.itemData != null)
            {
                slots[i].SetItem(item);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}