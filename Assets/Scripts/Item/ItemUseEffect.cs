using UnityEngine;

public abstract class ItemUseEffect : ScriptableObject
{
    public abstract bool Use(ItemUseContext context);
}

public struct ItemUseContext
{
    public GameObject Player;
    public Transform PlayerTransform;
    public Inventory Inventory;
    public Item Item;

    public ItemUseContext(GameObject player, Inventory inventory, Item item)
    {
        Player = player;
        PlayerTransform = player.transform;
        Inventory = inventory;
        Item = item;
    }
}