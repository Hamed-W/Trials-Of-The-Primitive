using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnOpenInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryPanel.activeSelf;

        if (!inventoryPanel.activeSelf)
        {
            craftingManager.ClearCraftingItems();
        }
    }

    void OnSplitStack(InputValue value)
    {
        if (value.isPressed)
        {
            inventory.splitStack = true;
        }
        else
        {
            inventory.splitStack = false;
        }
    }
}
