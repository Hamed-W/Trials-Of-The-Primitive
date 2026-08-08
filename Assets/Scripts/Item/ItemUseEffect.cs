using UnityEngine;

public abstract class ItemUseEffect : ScriptableObject
{
    public abstract bool Use(ItemUseContext context);
}

public struct ItemUseContext
{
    public GameObject player;
    public Transform playerTransform;
    public Inventory inventory;
    public Item item;

    public ItemUseContext(GameObject pPlayer, Inventory pInventory, Item pItem)
    {
        player = pPlayer;
        playerTransform = player.transform;
        inventory = pInventory;
        item = pItem;
    }
}