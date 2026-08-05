using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Sprite slotSprite;

    private Item item;

    public void SetItemSlot(Item newItem)
    {
        item = newItem;
        itemIcon.enabled = true;
        itemIcon.sprite = item.itemData.icon;
        nameText.text = item.itemData.name;
        quantityText.text = item.quantity.ToString();
    }

    public void ClearSlot()
    {
        item = null;
        itemIcon.sprite = slotSprite;
        quantityText.text = string.Empty;
    }
}