using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BiomeObjectRespawn : MonoBehaviour
{
    public Biome biome;

    public MapGenerator mapGenerator;
    public GameObject originalPrefab;

    public void Respawn()
    {
        mapGenerator.RespawnBiomeObject(originalPrefab, biome, transform.parent, 30f);
    }
}