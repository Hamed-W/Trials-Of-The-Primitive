using UnityEngine;

[CreateAssetMenu(
    fileName = "Heal Effect",
    menuName = "Inventory/Use Effects/Heal"
)]
public class HealingEffect : ItemUseEffect
{
    [SerializeField] private float healAmount = 25f;

    public override bool Use(ItemUseContext context)
    {
        Debug.Log("HEAL!!");
        return true;
    }
}