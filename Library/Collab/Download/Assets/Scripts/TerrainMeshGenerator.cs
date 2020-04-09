using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainMeshGenerator
{

    private static MeshData GenerateTerrainMeshData(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int LOD)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        int vertStep = LOD == 0 ? 1 : LOD * 2;
        int vertsPerLine = (width - 1) / vertStep + 1;

        MeshData meshData = new MeshData();
        int vertIndex = 0;

        for (int y = 0; y < height; y += vertStep) {
            for (int x = 0; x < width; x += vertStep) {
                meshData.verticles.Add(new Vector3(x, heightCurve.Evaluate(noiseMap[x, y]) * heightMultiplier, y));
                meshData.UVs.Add(new Vector2(x / (float)width, y / (float)height));

                if (y < height - 1 && x < width - 1) {
                    meshData.AddTriangle(vertIndex, vertIndex + vertsPerLine, vertIndex + vertsPerLine + 1);
                    meshData.AddTriangle(vertIndex + vertsPerLine + 1, vertIndex + 1, vertIndex);
                }

                vertIndex++;
            }
        }

        return meshData;
    }

    private static MeshData GenerateTerrainMeshData2(float[,] noiseMap, float heightMultiplier)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        MeshData meshData = new MeshData();
        int vertIndex = 0;

        for (int y = 0; y < height; y ++) {
            for (int x = 0; x < width; x ++) {
                meshData.verticles.Add(new Vector3(x, Mathf.Round(noiseMap[x, y] * 10) * heightMultiplier, y));
                meshData.UVs.Add(new Vector2(x / (float)width, y / (float)height));

                if (y < height - 1 && x < width - 1) {
                    meshData.AddTriangle(vertIndex, vertIndex + width, vertIndex + width + 1);
                    meshData.AddTriangle(vertIndex + width + 1, vertIndex + 1, vertIndex);
                }

                vertIndex++;
            }
        }

        return meshData;
    }


    public static Mesh CreateTerrain(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int LOD)
    {
        //MeshData meshData = GenerateTerrainMeshData(noiseMap, heightMultiplier, heightCurve, LOD);
        MeshData meshData = GenerateTerrainMeshData2(noiseMap, heightMultiplier);

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.verticles.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.UVs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

}

public class MeshData
{

    public List<Vector3> verticles = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> UVs = new List<Vector2>();

    public void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

}
