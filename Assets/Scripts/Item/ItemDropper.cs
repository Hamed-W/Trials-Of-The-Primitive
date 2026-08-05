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
            for (int i = 0; i < quantity; i++)
            {
                SpawnItem(drop.itemData, spawnPosition);
            }
        }
    }

    private void SpawnItem(ItemData itemData, Vector3 spawnPosition)
    {
        GameObject droppedItem = Instantiate(itemData.worldPrefab, spawnPosition, Random.rotation);
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        Vector3 direction =Vector3.up + Random.insideUnitSphere * 0.5f;
        rb.AddForce(direction.normalized * 3f, ForceMode.Impulse);
    }
}