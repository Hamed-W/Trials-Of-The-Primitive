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

    public float noiseScale;
    
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public TerrainMap terrainMap;

    public Material terrainMaterial;

    public bool autoUpdate;

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

        float[,] noiseMap = Noise.GenerateNoiseMap(mapVertexSize, mapVertexSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        terrainMap = new TerrainMap(mapVertexSize, transform, terrainMaterial, meshData.CreateMesh());
    }

    void OnValidate() {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }



    public class TerrainMap {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;


        public TerrainMap(int size, Transform parent, Material material, Mesh mesh)
        {
            meshObject = new GameObject("Terrain Map");

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.sharedMaterial = material;
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            meshObject.transform.SetParent(parent);
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.transform.localScale = Vector3.one;
        }
    }
}
