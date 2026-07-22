using UnityEngine;
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

        int mapVertexSize = (mapChunkSize - 1) * worldSize + 1;

        //float[,] heightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
        float[,] biomeMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed + 1000, biomeScale, 1, 0.5f, 2f, offset);

        float[,] grassHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed, grassNoiseScale, grassOctaves, grassPersistence, grassLacunarity, offset);
        float[,] desertHeightMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed + 2000, desertNoiseScale, desertOctaves, desertPersistence, desertLacunarity, offset);

        Texture2D biomeTexture = CreateBiomeTexture(biomeMap);

        float[,] finalHeightMap = BlendBiomeHeights(biomeMap, grassHeightMap, desertHeightMap);
        

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(finalHeightMap, levelOfDetail);
        terrainMap = new TerrainMap(mapVertexSize, transform, terrainMaterial, meshData.CreateMesh(), biomeTexture);
    }

    void OnValidate() {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
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

    private Texture2D CreateBiomeTexture(float[,] biomeMap)
    {
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

        texture.name = "Generated Biome Map";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] colours = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float biomeValue = biomeMap[x, y];

                colours[y * width + x] = new Color(biomeValue,biomeValue,biomeValue,1f);
            }
        }

        texture.SetPixels(colours);
        texture.Apply();

        return texture;
    }



    public class TerrainMap {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;


        public TerrainMap(int size, Transform parent, Material material, Mesh mesh, Texture2D biomeTexture)
        {
            meshObject = new GameObject("Terrain Map");

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            Material terrainMaterialInstance = new Material(material);

            Debug.Log(
                terrainMaterialInstance.HasProperty("_BiomeMap")
            );
            terrainMaterialInstance.SetTexture("_BiomeMap",biomeTexture);

            meshRenderer.sharedMaterial = terrainMaterialInstance;

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            meshObject.transform.SetParent(parent);
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.transform.localScale = Vector3.one;
        }
    }
}
