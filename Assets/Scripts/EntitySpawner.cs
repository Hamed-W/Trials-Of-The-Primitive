using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static MapGenerator;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private Transform player;

    [SerializeField] List<BiomePrefabs> entityPrefabs = new List<BiomePrefabs>();

    [SerializeField] private float minimumSpawnDistance = 20f;
    [SerializeField] private float maximumSpawnDistance = 35f;

    [SerializeField] private int baseEnemiesPerNight = 3;
    [SerializeField] private int enemiesAddedPerNight = 1;

    [SerializeField] private float spawnDelay = 1f;

    private List<GameObject> spawnedEntities = new List<GameObject>();

    [SerializeField] private MapGenerator mapGenerator;

    private Dictionary<Biome, GameObject> objectParents = new Dictionary<Biome, GameObject>();

    private Coroutine nightWaveCoroutine;

    //public int grassEntityCount = 200;
    //public int desertEntityCount = 100;

    void Awake()
    {
        foreach (BiomePrefabs biome in entityPrefabs)
        {
            objectParents.Add(biome.biome, new GameObject(biome.biome.ToString()));
        }
    }


    private void OnEnable()
    {
        dayNightCycle.OnNightStarted += ClearEntities;
        dayNightCycle.OnDayStarted += ClearEntities;

        dayNightCycle.OnNightStarted += StartNightWave;

        dayNightCycle.OnDayStarted += StopNightWave;
        dayNightCycle.OnDayStarted += StartDaySpawning;
    }

    private void OnDisable()
    {
        dayNightCycle.OnNightStarted -= ClearEntities;
        dayNightCycle.OnDayStarted -= ClearEntities;
        dayNightCycle.OnNightStarted -= StartNightWave;
        dayNightCycle.OnDayStarted -= StartDaySpawning;
    }

    private void StartNightWave()
    {
        nightWaveCoroutine = StartCoroutine(SpawnNightWave());
    }

    private void StopNightWave()
    {
        if (nightWaveCoroutine != null)
        {
            StopCoroutine(nightWaveCoroutine);
            nightWaveCoroutine = null;
        }
    }

    private void StartDaySpawning()
    {
        foreach (BiomePrefabs biome in entityPrefabs)
        {
            if (biome.biome == Biome.None) continue;
            spawnedEntities.AddRange(mapGenerator.SpawnObjectsForBiome(biome.dayPrefabs, biome.biomeCount, biome.biomeMapRangeStart, biome.biomeMapRangeEnd, objectParents[biome.biome].transform, 0));
        }
    }

    private IEnumerator SpawnNightWave()
    {
        int amount = baseEnemiesPerNight + dayNightCycle.dayCount * enemiesAddedPerNight;

        for (int i = 0; i < amount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }

        nightWaveCoroutine = null;
    }

    private void SpawnEnemy()
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        float distance = UnityEngine.Random.Range(minimumSpawnDistance, maximumSpawnDistance);
        Vector3 position = player.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * distance;

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas)) return;

        //Biome biome = mapGenerator.GetBiomeFromCoord(hit.position);
        //int biomeIndex = entityPrefabs.FindIndex(e => e.biome == biome);
        /*Debug.Log(biomeIndex);
        Debug.Log(entityPrefabs.Count);
        Debug.Log(biome);
        Debug.Log(entityPrefabs[biomeIndex].prefabs.Count);*/
        int biomeIndex = UnityEngine.Random.Range(0, entityPrefabs.Count);
        GameObject prefab = entityPrefabs[biomeIndex].nightPrefabs[UnityEngine.Random.Range(0, entityPrefabs[biomeIndex].nightPrefabs.Count)];
        GameObject enemy = Instantiate(prefab, hit.position, Quaternion.identity, objectParents[entityPrefabs[biomeIndex].biome].transform);
        spawnedEntities.Add(enemy);

        EntityBehaviour behaviour = enemy.GetComponent<EntityBehaviour>();

        if (behaviour != null)
        {
            behaviour.SetLevel(dayNightCycle.dayCount + 1);
            behaviour.SetAlwaysTargetPlayer(true);
        }

    }

    private void ClearEntities()
    {
        foreach (GameObject entity in spawnedEntities)
        {
            Destroy(entity);
        }
        spawnedEntities.Clear();
    }
}
public enum Biome
{
    None,
    Grass,
    Sand
}


[Serializable]
public class BiomePrefabs
{
    public Biome biome = Biome.None;
    public List<GameObject> dayPrefabs = new List<GameObject>();
    public List<GameObject> nightPrefabs = new List<GameObject>();
    public float biomeMapRangeStart;
    public float biomeMapRangeEnd;
    public int biomeCount = 0;
}
