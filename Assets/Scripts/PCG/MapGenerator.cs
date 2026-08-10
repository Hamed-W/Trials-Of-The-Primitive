using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, Mesh}
    public DrawMode drawMode;

    public const int mapChunkSize = 241;

    [Range(0,6)] // 241 - 1 = 240. 240 has 6 factors, 2,4,6,8,10,12. Multiply by 2 to get the increments (the factors stated) (increments are steps of vertices considered i.e. i=2 means skip every 2)
    public int levelOfDetail;
    
    //public int mapWidth; Replaced by mapChunkSize.
    //public int mapHeight; Replaced by mapChunkSize.
    
    public int worldSize = 1;

    public float biomeScale = 600f;

    public float noiseScale;
    
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;


    public TerrainMap terrainMap;

    public Material terrainMaterial;

    public bool autoUpdate;


    public float grassNoiseScale = 35f;
    public int grassOctaves = 4;
    public float grassPersistence = 0.5f;
    public float grassLacunarity = 2f;

    public float desertNoiseScale = 90f;
    public int desertOctaves = 2;
    public float desertPersistence = 0.35f;
    public float desertLacunarity = 1.8f;

    [Header("Grass Height")]
    public float grassHeightMultiplier = 10f;
    public AnimationCurve grassHeightCurve;

    [Header("Desert Height")]
    public float desertHeightMultiplier = 6f;
    public AnimationCurve desertHeightCurve;

    public int grassObjectCount = 200;
    public int desertObjectCount = 100;

    public float objectHeightOffset = 0f;

    public LayerMask objectLayerMask;
    public float minimumObjectSpacing = 2f;


    public LayerMask terrainLayerMask;
    
    public float maximumObjectSlope = 30f;

    [SerializeField] private NavMeshSurface navMeshSurface;

    private float[,] biomeMap;
    private float[,] finalHeightMap;
    private int mapVertexSize;

    public List<BiomePrefabs> biomeObjectPrefabs = new List<BiomePrefabs>();

    private List<GameObject> biomeObjects = new List<GameObject>();

    [SerializeField] private GameObject objectParent;



    [SerializeField] private float terrainMorphDuration = 3f;

    [SerializeField] private DayNightCycle dayNightCycle;

    [SerializeField] private Transform player;

    public bool isMorphing = false;


    void Awake()
    {
        dayNightCycle.OnDayStarted += SwitchToDay;
        dayNightCycle.OnNightStarted += SwitchToNight;
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(MorphTerrain(finalHeightMap, CreateNightHeightMap(0f), biomeMap, CreateNightBiomeMap(1f)));
        }
    }


    public void GenerateMap()
    {

        //Deletes old children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        mapVertexSize = (mapChunkSize - 1) * worldSize + 1;

        //float[,] heightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
        biomeMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed + 1000, biomeScale, 1, 0.5f, 2f, offset);

        float[,] grassHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed, grassNoiseScale, grassOctaves, grassPersistence, grassLacunarity, offset);
        float[,] desertHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed + 2000, desertNoiseScale, desertOctaves, desertPersistence, desertLacunarity, offset);

        Texture2D biomeTexture = UpdateBiomeTexture(biomeMap);

        finalHeightMap = BlendBiomeHeights(biomeMap, grassHeightMap, desertHeightMap);
        

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(finalHeightMap, levelOfDetail);

        Mesh generatedMesh = meshData.CreateMesh();

        terrainMap = new TerrainMap(mapVertexSize, transform, terrainMaterial, generatedMesh, biomeTexture);

        Physics.SyncTransforms();

        SpawnBiomeObjects();

        Physics.SyncTransforms();

        if (navMeshSurface != null)
        {
            navMeshSurface.RemoveData();
            navMeshSurface.BuildNavMesh();
        }

        //SpawnBiomeObjects(biomeMap, finalHeightMap, mapVertexSize, "Entities");
    }

    void OnValidate() {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }


    public void SpawnBiomeObjects()
    {
        foreach (BiomePrefabs biome in biomeObjectPrefabs)
        {
            if (biome.biome == Biome.None) continue;
            biomeObjects.AddRange(SpawnObjectsForBiome(biome.dayPrefabs, biome.biomeCount, biome.biomeMapRangeStart, biome.biomeMapRangeEnd, objectParent.transform, objectLayerMask));
        }
    }

    public List<GameObject> SpawnObjectsForBiome(List<GameObject> prefabs, int objectCount, float minimumBiomeValue, float maximumBiomeValue, Transform parent, LayerMask layerMask)
    {
        if (prefabs == null || prefabs.Count == 0)
            return new List<GameObject>();

        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        float topLeftX = (mapVertexSize - 1) / -2f;
        float topLeftZ = (mapVertexSize - 1) / 2f;

        int spawned = 0;
        int attempts = 0;

        int maximumAttempts = objectCount * 20;
        List<GameObject> spawnedObjects = new List<GameObject>();

        while (spawned < objectCount && attempts < maximumAttempts)
        {
            attempts++;

            //int x = Random.Range(0, width);
            //int y = Random.Range(0, height);

            /*float worldX = topLeftX + x;
            float worldZ = topLeftZ - y;
            float worldY = heightMap[x, y] + objectHeightOffset;

            Vector3 position = new Vector3(worldX, worldY, worldZ);*/

            float worldX = UnityEngine.Random.Range(topLeftX, -topLeftX);

            float worldZ = UnityEngine.Random.Range(-topLeftZ, topLeftZ);

            Vector3 rayStart = new Vector3(worldX, 1000f, worldZ);

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2000f, terrainLayerMask))
            {
                continue;
            }

            Vector3 position = hit.point + Vector3.up * objectHeightOffset;

            //Convert centred world coordinates back into map indices.
            int mapX = Mathf.RoundToInt(worldX - topLeftX);

            int mapY = Mathf.RoundToInt(topLeftZ - worldZ);

            //Protect against rounding beyond the array boundaries.
            mapX = Mathf.Clamp(mapX, 0, width - 1);
            mapY = Mathf.Clamp(mapY, 0, height - 1);

            float biomeValue = biomeMap[mapX, mapY];


            if (biomeValue < minimumBiomeValue || biomeValue > maximumBiomeValue)
            {
                continue;
            }

            float slope = GetSlope(finalHeightMap, mapX, mapY);

            if (slope > maximumObjectSlope)
                continue;

            if (Physics.CheckSphere(position, minimumObjectSpacing, layerMask))
            {
                continue;
            }

            GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];

            GameObject spawnedObject = Instantiate(prefab, position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), parent);

            float randomScale = UnityEngine.Random.Range(0.8f, 1.2f);

            spawnedObject.transform.localScale *= randomScale;

            spawnedObjects.Add(spawnedObject);

            spawned++;
        }

        Debug.Log("spawned");

        return spawnedObjects;
    }

    private float GetSlope(float[,] heightMap,int x,int y)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        int left = Mathf.Max(x - 1, 0);
        int right = Mathf.Min(x + 1, width - 1);
        int down = Mathf.Max(y - 1, 0);
        int up = Mathf.Min(y + 1, height - 1);

        float differenceX = heightMap[right, y] - heightMap[left, y];

        float differenceY = heightMap[x, up] - heightMap[x, down];

        Vector3 normal = new Vector3(-differenceX, 2f, differenceY).normalized;

        return Vector3.Angle(normal, Vector3.up);
    }






    private float[,] BlendBiomeHeights(float[,] biomeMap, float[,] grassMap, float[,] desertMap)
    {
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        float[,] result = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float biomeValue = biomeMap[x, y];

                float biomeBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 0.6f, biomeValue));

                float grassHeight = grassHeightCurve.Evaluate(grassMap[x, y]) * grassHeightMultiplier;

                float desertHeight = desertHeightCurve.Evaluate(desertMap[x, y]) * desertHeightMultiplier;

                result[x, y] = Mathf.Lerp(grassHeight, desertHeight, biomeBlend);
            }
        }

        return result;
    }




    //Converts noise values to black-white colour in a texture that the shadergraph can sample and use to blend.

    private Texture2D UpdateBiomeTexture(float[,] biomeMap)
    {
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        Texture2D texture = null;
        if (terrainMap == null)
        {
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            texture.name = "Generated Biome Map";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
        }

        Color[] colours = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float biomeValue = biomeMap[x, y];

                colours[y * width + x] = new Color(biomeValue,biomeValue,biomeValue,1f);
            }
        }
        if (terrainMap != null)
        {
            terrainMap.biomeTexture.SetPixels(colours);
            terrainMap.biomeTexture.Apply();
        }
        else
        {
            texture.SetPixels(colours);
            texture.Apply();
        }

        return texture;
    }





    public Biome GetBiomeFromCoord(Vector3 position)
    {
        int mapVertexSize = (mapChunkSize - 1) * worldSize + 1;
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        float topLeftX = (mapVertexSize - 1) / -2f;
        float topLeftZ = (mapVertexSize - 1) / 2f;

        float worldX = position.x;
        float worldZ = position.z;

        //Convert centred world coordinates back into map indices.
        int mapX = Mathf.RoundToInt(worldX - topLeftX);

        int mapY = Mathf.RoundToInt(topLeftZ - worldZ);

        //Protect against rounding beyond the array boundaries.
        mapX = Mathf.Clamp(mapX, 0, width - 1);
        mapY = Mathf.Clamp(mapY, 0, height - 1);

        float biomeValue = biomeMap[mapX, mapY];


        if (biomeValue > 0f && biomeValue < 0.4f)
        {
            return Biome.Grass;
        }
        else if (biomeValue > 0.6 && biomeValue < 1)
        {
            return Biome.Sand;
        }
        else
        {
            return Biome.None;
        }
    }

    private float[,] GenerateNewHeightMap(int newSeed, float[,] targetBiomeMap)
    {
        float[,] grassHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, newSeed, grassNoiseScale, grassOctaves, grassPersistence, grassLacunarity, offset);

        float[,] desertHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, newSeed + 2000, desertNoiseScale, desertOctaves, desertPersistence, desertLacunarity, offset);

        return BlendBiomeHeights(targetBiomeMap, grassHeightMap, desertHeightMap);
    }

    private float[,] GenerateNewBiomeMap(int newSeed)
    {
        return Noise.GenerateNoiseMap(mapVertexSize,mapVertexSize, newSeed + 1000, biomeScale, 1, 0.5f, 2f, offset);
    }




    private float[,] CreateNightHeightMap(float height)
    {
        int width = finalHeightMap.GetLength(0);
        int mapHeight = finalHeightMap.GetLength(1);

        float[,] flatHeightMap =
            new float[width, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flatHeightMap[x, y] = height;
            }
        }

        return flatHeightMap;
    }

    private float[,] CreateNightBiomeMap(float value)
    {
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        float[,] nightBiomeMap =
            new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                nightBiomeMap[x, y] = value;
            }
        }

        return nightBiomeMap;
    }


    private void UpdateTerrainHeights(float[,] heightMap, bool finalUpdate)
    {
        Mesh mesh = terrainMap.meshFilter.sharedMesh;

        Vector3[] vertices = mesh.vertices;

        int increment = levelOfDetail == 0 ? 1: levelOfDetail * 2;

        int vertexIndex = 0;

        for (int y = 0; y < heightMap.GetLength(1); y += increment)
        {
            for (int x = 0; x < heightMap.GetLength(0); x += increment)
            {
                vertices[vertexIndex].y = heightMap[x, y];
                vertexIndex++;
            }
        }

        mesh.vertices = vertices;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (finalUpdate)
        {
            MeshData colliderMeshData = MeshGenerator.GenerateTerrainMesh(heightMap, 1);
            Mesh colliderMesh = colliderMeshData.CreateMesh();

            terrainMap.meshCollider.sharedMesh = null;
            terrainMap.meshCollider.sharedMesh = colliderMesh;
        }
        //terrainMap.meshCollider.sharedMesh = null;
    }


    private IEnumerator MorphTerrain(float[,] startHeightMap, float[,] targetHeightMap, float[,] startBiomeMap, float[,] targetBiomeMap)
    {
        player.GetComponent<PlayerMovement>().RemoveGravity();
        player.GetComponent<PlayerMovement>().isMorphing = true;
        isMorphing = true;
        float elapsed = 0f;

        int width = startHeightMap.GetLength(0);
        int height = startHeightMap.GetLength(1);

        float[,] currentHeights = new float[width, height];
        float[,] currentBiomeMap = new float[width, height];

        while (elapsed < terrainMorphDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / terrainMorphDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    currentHeights[x, y] = Mathf.Lerp(startHeightMap[x, y], targetHeightMap[x, y], t);
                    currentBiomeMap[x, y] = Mathf.Lerp(startBiomeMap[x, y], targetBiomeMap[x, y], t);
                }
            }

            UpdateTerrainHeights(currentHeights, false);
            UpdateBiomeTexture(currentBiomeMap);
            UpdatePlayerHeight(currentHeights);
            yield return null;
        }

        UpdateTerrainHeights(targetHeightMap, true);
        UpdateBiomeTexture(targetBiomeMap);
        UpdatePlayerHeight(targetHeightMap);

        finalHeightMap = targetHeightMap;
        biomeMap = targetBiomeMap;
        // Rebuild the navmesh



        //navMeshSurface.RemoveData();
        //navMeshSurface.BuildNavMesh();
        yield return StartCoroutine(UpdateNavMeshAsync());


        player.GetComponent<PlayerMovement>().isMorphing = false;
        isMorphing = false;
        player.GetComponent<PlayerMovement>().RestoreGravity();
    }

    private IEnumerator UpdateNavMeshAsync()
    {
        AsyncOperation operation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

        yield return operation;
    }

    private float GetHeightFromCoordinate(float[,] heightMap, Vector3 position)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (mapVertexSize - 1) / -2f;
        float topLeftZ = (mapVertexSize - 1) / 2f;

        int mapX = Mathf.RoundToInt(position.x - topLeftX);
        int mapY = Mathf.RoundToInt(topLeftZ - position.z);

        mapX = Mathf.Clamp(mapX, 0, width - 1);
        mapY = Mathf.Clamp(mapY, 0, height - 1);

        return heightMap[mapX, mapY];
    }

    private void UpdatePlayerHeight(float[,] heightMap)
    {
        Vector3 position = player.position;
        position.y = GetHeightFromCoordinate(heightMap, position) + 2.65f;
        player.position = position;
    }



    private void DestroyBiomeObjects()
    {
        foreach (GameObject biomeObject in biomeObjects)
        {
            Destroy(biomeObject);
        }
        biomeObjects.Clear();
    }


    public void ChangeSeed()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public void SwitchToDay()
    {
        StartCoroutine(SwitchToDayCoroutine());
    }

    private IEnumerator SwitchToDayCoroutine()
    {
        ChangeSeed();

        float[,] newBiomeMap = GenerateNewBiomeMap(seed);

        float[,] newHeightMap = GenerateNewHeightMap(seed, newBiomeMap);

        yield return StartCoroutine(MorphTerrain(finalHeightMap, newHeightMap, biomeMap, newBiomeMap));
        
        //Waits for the generation to finish BEFORE generating new objects.
        SpawnBiomeObjects();
    }

    public void SwitchToNight()
    {
        StartCoroutine(SwitchToNightCoroutine());
    }

    private IEnumerator SwitchToNightCoroutine()
    {
        DestroyBiomeObjects();

        yield return StartCoroutine(MorphTerrain(finalHeightMap, CreateNightHeightMap(0f), biomeMap, CreateNightBiomeMap(1f)));
    }








    public class TerrainMap {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;
        public Texture2D biomeTexture;


        public TerrainMap(int size, Transform parent, Material material, Mesh mesh, Texture2D biomeTexture)
        {
            meshObject = new GameObject("Terrain Map");

            this.biomeTexture = biomeTexture;
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            Material terrainMaterialInstance = new Material(material);

            terrainMaterialInstance.SetTexture("_BiomeMap", biomeTexture);

            meshRenderer.sharedMaterial = terrainMaterialInstance;

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            meshObject.transform.SetParent(parent);
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.transform.localScale = Vector3.one;
            meshObject.layer = LayerMask.NameToLayer("Terrain");
        }
    }
}
