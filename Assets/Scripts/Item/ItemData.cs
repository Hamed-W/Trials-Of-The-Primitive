using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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
    public float itemUseAmount; // Amount of healing, eating or *base* damage for a sword.

    [Header("Equipment")]
    public bool equippable;
    public EquipmentAttachment attachment;
    public GameObject equippedPrefab;

    public List<EquipmentStatModifiers> statModifiers;
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

[System.Serializable]
public class EquipmentStatModifiers
{
    public PlayerStatType statType;
    public float amount;
}