using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public CraftingManager craftingManager;
    public int slotIndex;

    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Sprite slotSprite;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas rootCanvas;
    private RectTransform draggedIcon;

    public bool droppedOnInventorySlot;

    public void SetItem(Item item)
    {
        itemIcon.sprite = item.itemData.icon;
        itemIcon.enabled = true;
        quantityText.text = item.quantity.ToString();
    }

    public void ClearSlot()
    {
        itemIcon.sprite = slotSprite;
        quantityText.text = "";
    }

    public void OnBeginDrag(PointerEventData data)
    {
        Item draggedItem = craftingManager.craftingItems[slotIndex];

        if (draggedItem == null) return;

        droppedOnInventorySlot = false;

        CreateDraggedIcon(draggedItem.itemData.icon);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData data)
    {
        if (draggedIcon == null) return;

        draggedIcon.position = data.position;
    }

    public void OnDrop(PointerEventData data)
    {
        InventorySlot sourceSlot = data.pointerDrag.GetComponent<InventorySlot>();
        CraftingSlot craftingSlot = data.pointerDrag.GetComponent<CraftingSlot>();

        if (sourceSlot != null)
        {
            sourceSlot.droppedOnInventorySlot = true;
            craftingManager.SwapInventoryAndCrafting(sourceSlot.slotIndex, slotIndex);

        }
        if (craftingSlot != null)
        {
            craftingSlot.droppedOnInventorySlot = true;
            craftingManager.SwapItems(slotIndex, craftingSlot.slotIndex);
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        canvasGroup.blocksRaycasts = true;

        if (!droppedOnInventorySlot) craftingManager.DropItem(slotIndex); ;

        DestroyDraggedIcon();
    }

    private void CreateDraggedIcon(Sprite sprite)
    {
        GameObject iconObject = new GameObject("Dragged Item Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(rootCanvas.transform, false);
        draggedIcon = iconObject.GetComponent<RectTransform>();
        draggedIcon.sizeDelta = itemIcon.rectTransform.rect.size;
        draggedIcon.position = itemIcon.rectTransform.position;
        Image draggedImage = iconObject.GetComponent<Image>();
        draggedImage.sprite = sprite;
        draggedImage.preserveAspect = true;
        draggedImage.raycastTarget = false;
        iconObject.transform.SetAsLastSibling();
    }

    private void DestroyDraggedIcon()
    {
        if (draggedIcon == null) return;

        Destroy(draggedIcon.gameObject);
        draggedIcon = null;
    }
}