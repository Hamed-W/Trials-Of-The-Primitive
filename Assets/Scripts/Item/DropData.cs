using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropData
{
    public ItemData itemData;

    [Range(0f, 1f)]
    public float dropChance = 1f;

    public int minimumQuantity = 1;
    public int maximumQuantity = 1;
}