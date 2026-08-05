using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item Data",
    menuName = "Inventory/Item Data"
)]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;

    public bool stackable = true;
    public int maximumStackSize = 99;

    public GameObject worldPrefab;
}