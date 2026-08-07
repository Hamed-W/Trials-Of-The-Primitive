using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] private List<DropData> possibleDrops;
    [SerializeField] private Transform spawnPoint;

    public void DropItems()
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;

        foreach (DropData drop in possibleDrops)
        {
            if (Random.value > drop.dropChance)
                continue;

            int quantity = Random.Range(drop.minimumQuantity, drop.maximumQuantity + 1); // +1 to make it inclusive.
            while (quantity > 0)
            {
                int spawnQuantity = Mathf.Min(quantity, drop.itemData.maximumStackSize);
                SpawnItem(drop.itemData, spawnPosition, spawnQuantity);
                quantity -= spawnQuantity;
            }
        }
    }

    private void SpawnItem(ItemData itemData, Vector3 spawnPosition, int quantity)
    {
        GameObject droppedItem = Instantiate(itemData.worldPrefab, spawnPosition, Random.rotation);

        PickUpItem pickUpItem = droppedItem.GetComponent<PickUpItem>();
        pickUpItem.SetQuantity(quantity);

        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();

        Vector3 direction = Vector3.up + Random.insideUnitSphere * 0.5f;
        rb.AddForce(direction.normalized * 3f, ForceMode.Impulse);
    }
}