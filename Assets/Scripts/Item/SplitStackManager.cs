using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SplitStackManager : MonoBehaviour
{
    public Item selectedItem;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Inventory inventory;
    private int newAmount = 0;
    public int index;

    // Updates the text to display the amount that we want to split into
    public void UpdateAmount(float percentage)
    {
        newAmount = Mathf.RoundToInt(selectedItem.quantity * percentage);
        quantityText.text = newAmount.ToString() + " / " + selectedItem.quantity.ToString();
        Debug.Log(newAmount.ToString());
    }

    public void ConfirmSplit()
    {
        if (selectedItem != null && newAmount > 0)
        {
            inventory.SplitItemStack(selectedItem, newAmount, index);
        }
        selectedItem = null;
        quantityText.text = "-Hold Slider-";
        newAmount = 0;
        gameObject.SetActive(false);
    }
}
