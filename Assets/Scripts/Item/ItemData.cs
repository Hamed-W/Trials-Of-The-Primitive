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

    public int maximumStackSize = 99;

    public GameObject worldPrefab;

    public ItemUseType itemUseType;
    public ItemUseEffect useEffect;

    [Header("Equipment")]
    public bool equippable;
    public EquipmentAttachment attachment;
    public GameObject equippedPrefab;
}

public enum EquipmentAttachment
{
    None,
    RightHand,
    LeftHand,
    Head,
    Back,
    Chest
}

public enum ItemUseType
{
    None,
    Swing,
    Use,
    Consume,
    Shield,
    Helmet,
    Armor
}