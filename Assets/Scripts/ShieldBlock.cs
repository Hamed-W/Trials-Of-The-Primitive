using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ShieldBlock : MonoBehaviour
{

    private GameObject player;
    public ItemData itemData;
    public PlayerStats playerStats;

    void Awake()
    {
        player = transform.root.gameObject;
    }

    void OnTriggerStay(Collider other)
    {
        EntityStats entity = other.GetComponentInParent<EntityStats>();
        if (entity == null) return;
        if (!player.GetComponent<ItemUseController>().isBlocking)
        {
            entity.RemoveWeaken();
        }
        else
        {
            entity.Weaken(itemData.itemUseAmount);
        }
        //entity.blocked = player.GetComponent<ItemUseController>().isBlocking;
    }

    void OnTriggerExit(Collider other)
    {
        EntityStats entity = other.GetComponentInParent<EntityStats>();
        if (entity == null) return;
        //entity.blocked = false;
        entity.RemoveWeaken();
    }
}
