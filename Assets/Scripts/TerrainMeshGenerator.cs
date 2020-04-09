using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Dynamic;

public static class TerrainMeshGenerator
{ 
    private static MeshData GenerateTerrainMeshData(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int LOD)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        MeshData meshData = new MeshData(width, height);

        int borderedVertIndex = -1;
        int meshVertIndex = 0;
        int[,] vertIndicesMap = new int[width, height];

        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++) {
                bool isBorderedVertices = i == 0 || i == width - 1 || j == 0 || j == height - 1;
                if (isBorderedVertices) {
                    vertIndicesMap[i, j] = borderedVertIndex;
                    borderedVertIndex--;
                } else {
                    vertIndicesMap[i, j] = meshVertIndex;
                    meshVertIndex++;
                }
            }
        } 

        for (int j = 0; j < height; j++ ) {
            for (int i = 0; i < width; i++) {

                float x = i / (float)LOD;
                float z = j / (float)LOD;
                int vertIndex = vertIndicesMap[i, j];
                float value = heightCurve.Evaluate(noiseMap[i, j]) * heightMultiplier;

                if (vertIndex < 0) {
                    meshData.AddBorderedVertices(x, value, z);
                } else {
                    meshData.AddVertices(x, value, z, width / LOD, height / LOD);
                }

                if (j < height - 1 && i < width - 1) {
                    meshData.AddTriangle(vertIndicesMap[i, j], vertIndicesMap[i, j + 1], vertIndicesMap[i + 1, j + 1]);
                    meshData.AddTriangle(vertIndicesMap[i + 1, j + 1], vertIndicesMap[i + 1, j], vertIndicesMap[i, j]);
                }                

            }
        }

        Debug.Log("Verticles count: " + meshVertIndex.ToString());
        return meshData;
    }


    public static Mesh CreateTerrain(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int LOD)
    {
        MeshData meshData = GenerateTerrainMeshData(noiseMap, heightMultiplier, heightCurve, LOD);

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.GetVertices();
        mesh.triangles = meshData.GetTriangles();
        mesh.uv = meshData.GetUV();
        mesh.normals = meshData.CalculateNormals();

        return mesh;
    }

}

public class MeshData
{
    int width;
    int height;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> borderVertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> borderTriangles = new List<int>();
    List<Vector2> UV = new List<Vector2>();

    public MeshData(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public Vector3[] CalculateNormals()
    {
        Vector3[] normals = new Vector3[vertices.Count];

        int meshTriangleCount = triangles.Count / 3;
        for (int i = 0; i < meshTriangleCount; i++) {
            int meshVertIndex = i * 3;
            int a = triangles[meshVertIndex];
            int b = triangles[meshVertIndex + 1];
            int c = triangles[meshVertIndex + 2];
            Vector3 normal = CalculateNormal(a, b, c);
            normals[a] += normal;
            normals[b] += normal;
            normals[c] += normal;
        }

        int borderTriangleCount = borderTriangles.Count / 3;
        for (int i = 0; i < borderTriangleCount; i++) {
            int borderVertIndex = i * 3;
            int a = borderTriangles[borderVertIndex];
            int b = borderTriangles[borderVertIndex + 1];
            int c = borderTriangles[borderVertIndex + 2];
            Vector3 normal = CalculateNormal(a, b, c);
            if (a > 0)
                normals[a] += normal;
            if (b > 0)
                normals[b] += normal;
            if (c > 0)
                normals[c] += normal;
        }

        return normals;
    }

    Vector3 CalculateNormal(int a, int b, int c)
    {
        Vector3 vA = a < 0 ? borderVertices[-a - 1] : vertices[a];
        Vector3 vB = b < 0 ? borderVertices[-b - 1] : vertices[b];
        Vector3 vC = c < 0 ? borderVertices[-c - 1] : vertices[c];

        Vector3 sideAB = vB - vA;
        Vector3 sideAC = vC - vA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void AddVertices(float x, float y, float z, float width, float height)
    {
        vertices.Add(new Vector3(x, y, z));
        UV.Add(new Vector2(x / width, z / height));
    }

    public void AddBorderedVertices(float x, float y, float z)
    {
        borderVertices.Add(new Vector3(x, y, z));
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0) {
            borderTriangles.Add(a);
            borderTriangles.Add(b);
            borderTriangles.Add(c);
        } else {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
    }

    public Vector3[] GetVertices()
    {
        return vertices.ToArray();
    }

    public Vector3[] GetBorderedVertices()
    {
        return borderVertices.ToArray();
    }

    public int[] GetTriangles()
    {
        return triangles.ToArray();
    }

    public Vector2[] GetUV()
    {
        return UV.ToArray();
    }

}
