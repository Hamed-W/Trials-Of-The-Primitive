using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Eating Effect",
    menuName = "Inventory/Use Effects/Eat"
)]

public class EatingEffect : ItemUseEffect
{
    public override bool Use(ItemUseContext context)
    {
        Debug.Log("EAT!!");
        PlayerStats stats = context.player.GetComponent<PlayerStats>();

        return stats.RestoreHunger(context.item.itemData.itemUseAmount);
    }
}