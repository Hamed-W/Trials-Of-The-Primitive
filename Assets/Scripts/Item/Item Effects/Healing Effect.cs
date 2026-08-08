using UnityEngine;

[CreateAssetMenu(
    fileName = "Heal Effect",
    menuName = "Inventory/Use Effects/Heal"
)]
public class HealingEffect : ItemUseEffect
{
    public override bool Use(ItemUseContext context)
    {
        Debug.Log("HEAL!!");
        PlayerStats stats = context.player.GetComponent<PlayerStats>();

        return stats.Heal(context.item.itemData.itemUseAmount);
    }
}