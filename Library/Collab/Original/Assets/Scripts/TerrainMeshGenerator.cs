using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class TerrainMeshGenerator
{ 
    private static MeshData GenerateTerrainMeshData(float[,] noiseMap, float heightMultiplier, float[,] secondNoiseMap, float influencePower, int levelOfDetail)
    {
        int LOD = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        MeshData meshData = new MeshData();
        int vertIndex = 0;

        for (int j = 0; j < height; j++ ) {
            for (int i = 0; i < width; i++) {

                float x = i / (float)LOD;
                float y = j / (float)LOD;

                //float value = Mathf.Round(noiseMap[i, j] * 10) * heightMultiplier;
                float value = noiseMap[i, j] * heightMultiplier;
                if (secondNoiseMap != null)
                    value += secondNoiseMap[i, j] * influencePower;

                meshData.verticles.Add(new Vector3(x,  value, y));
                meshData.UVs.Add(new Vector2(x / (width / LOD), y / (height / LOD)));

                if (j < height - 1 && i < width - 1) {
                    meshData.AddTriangle(vertIndex, vertIndex + width, vertIndex + width + 1);
                    meshData.AddTriangle(vertIndex + width + 1, vertIndex + 1, vertIndex);
                }

                vertIndex++;
            }
        }

        Debug.Log("Verticles count: " + vertIndex.ToString());
        return meshData;
    }


    public static Mesh CreateTerrain(float[,] noiseMap, float heightMultiplier, float[,] secondNoiseMap, float influencePower, int levelOfDetail)
    {
        MeshData meshData = GenerateTerrainMeshData(noiseMap, heightMultiplier, secondNoiseMap, influencePower, levelOfDetail);

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
