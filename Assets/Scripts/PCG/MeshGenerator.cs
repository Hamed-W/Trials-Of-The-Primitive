using UnityEngine;
using UnityEngine.Rendering;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f; //X value 
        float topLeftZ = (height - 1) / 2f; //Z value is positive as we go up, so positive 2f.

        int meshSimplificationIncrement = (levelOfDetail == 0 ? 1 : levelOfDetail * 2);
        int verticesPerLine = (width - 1)/meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y=0; y<height; y+=meshSimplificationIncrement)
        {
            for (int x=0; x<width; x+=meshSimplificationIncrement){

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x , heightMap[x,y], topLeftZ - y); //Was (0,0) on top left, now (-1, 1) and (0,0) correctly at center.
                meshData.uvs[vertexIndex] = new Vector2(x/(float)(width-1),y/(float)(height-1));

                if (x < width-1 && y < height -1) //Ignoring the right and bottom edge of map (no triangles to the right or below).
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        return meshData;
    }
}
public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1)*(meshHeight-1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.indexFormat = IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
