using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Sprite slotSprite;

    [SerializeField] protected bool draggable = true;

    private Inventory inventory;
    private Canvas rootCanvas;
    public int slotIndex;

    private Item item;

    private RectTransform draggedIcon;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform inventoryPanel;
    public bool droppedOnInventorySlot;

    [SerializeField] private GameObject splitStackPanel;
    private SplitStackManager splitStackManager;

    public void SetItemSlot(Item newItem)
    {
        if (newItem == null || newItem.itemData == null)
        {
            ClearSlot();
            return;
        }

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
        nameText.text = string.Empty;
    }

    public void Initialise(Inventory pInventory, int pSlotIndex, Canvas pRootCanvas, RectTransform pInventoryPanel, GameObject pSplitStackPanel)
    {
        if (!draggable) return;
        inventory = pInventory;
        slotIndex = pSlotIndex;
        rootCanvas = pRootCanvas;
        inventoryPanel = pInventoryPanel;
        splitStackPanel = pSplitStackPanel;            
    }


    /*
     * Drag and Drop functionality
     * 
     * Holding left click onto a slot will make that InventorySlot instance to be considered in a dragged state. We create a dragged icon as a separate image as soon as the drag starts.
     * We then change the position of the dragged icon to follow the mouse pointer position while left click is being held. The source InventorySlot that we initially started dragging from will have its blocksRaycasts property set to false so that it can be dropped onto another InventorySlot.
     * Essentially, the dragged icon is just a visual representation of the item being dragged, while the actual item remains in the source InventorySlot until it is dropped onto another InventorySlot.
     * 
     * OnBeginDrag, OnDrag and OnEndDrag are called only on the GameObject being dragged: the source InventorySlot.
     * 
     * OnDrop is called only in the GameObject that the drag is being dropped onto.
     * By doing data.pointerDrag?.GetComponent<InventorySlot>(), we can get the source InventorySlot that is being dragged and dropped onto this InventorySlot.
     * 
     */

    public void OnBeginDrag(PointerEventData data)
    {
        if (!draggable) return;
        Item draggedItem = inventory.items[slotIndex];

        if (draggedItem == null) return;

        droppedOnInventorySlot = false;

        CreateDraggedIcon(draggedItem.itemData.icon);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!draggable) return;
        if (draggedIcon == null) return;

        draggedIcon.position = data.position;
    }

    public void OnDrop(PointerEventData data)
    {
        if (!draggable) return;
        InventorySlot sourceSlot = data.pointerDrag.GetComponent<InventorySlot>();
        CraftingSlot craftingSlot = data.pointerDrag.GetComponent<CraftingSlot>();

        //if (sourceSlot == null && craftingSlot == null) return; // In case I have any other draggable UI elements that won't have InventorySlot.

        if (sourceSlot != null)
        {
            sourceSlot.droppedOnInventorySlot = true;
            inventory.SwapItems(sourceSlot.slotIndex, slotIndex);
        }
        else if (craftingSlot != null)
        {
            craftingSlot.droppedOnInventorySlot = true;
            craftingSlot.craftingManager.SwapInventoryAndCrafting(slotIndex, craftingSlot.slotIndex);
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (!draggable) return;
        canvasGroup.blocksRaycasts = true;

        if (!droppedOnInventorySlot) inventory.DropItem(slotIndex); ;

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

    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Right) return;

        Item clickedItem = inventory.items[slotIndex];

        if (clickedItem == null || clickedItem.itemData == null)
        {
            return;
        }

        if (clickedItem.itemData.equippable) inventory.EquipItem(clickedItem);
    }

    public void OnSlotClick()
    {
        Debug.Log("Hi 1");
        if (inventory.splitStack) splitStackPanel.SetActive(true);
        Debug.Log("Hi 2");
        splitStackManager = splitStackPanel.GetComponent<SplitStackManager>();
        splitStackManager.selectedItem = inventory.items[slotIndex];
        splitStackManager.index = slotIndex;
    }
}