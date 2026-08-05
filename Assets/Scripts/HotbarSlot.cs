using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class HotbarSlotUI : InventorySlot
{
    [SerializeField] private Outline selectedOutline;

    public void SetSelected(bool selected)
    {
        selectedOutline.enabled = selected;
    }
}