using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainObjectManager : MonoBehaviour
{
    System.Random prng;
    public List<TerrainObjectData> objects;

    int LOD;
    MapGenerator mapGenerator;

    public void GenerateObjects(float[,] heightMap, MapGenerator mapGenerator)
    {
        prng = new System.Random(mapGenerator.seed);
        this.mapGenerator = mapGenerator;
        int chunkCount = mapGenerator.mapSizeX * mapGenerator.mapSizeY;
        LOD = MapGenerator.LOD;

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Dictionary<int, List<Vector2>> objectsMap = new Dictionary<int, List<Vector2>>();

        for (int i = 0; i < objects.Count; i++) {
            objectsMap.Add(i, PoissonDiskSampling.GeneratePoints(mapGenerator.seed, width, height, LOD, chunkCount, objects[i].spawnRadius, objects[i].spawnIterationCount));
            SpawnObjects(objectsMap[i], objects[i], heightMap);
        }
    }

    void SpawnObjects(List<Vector2> points, TerrainObjectData obj, float[,] heightMap)
    {
        float cellSize = 1f / LOD;

        foreach (Vector2 point in points) {
            PointData pointData = CalculatePointData(heightMap, cellSize, point);
            bool spawnAllowed = true;
            spawnAllowed &= pointData.height >= obj.minSpawnHeight && pointData.height <= obj.maxSpawnHeight;
            spawnAllowed &= Mathf.Abs(pointData.gradient.x) <= obj.maxSpawnSlopeValue && Mathf.Abs(pointData.gradient.y) <= obj.maxSpawnSlopeValue;
            if (spawnAllowed) {
                Vector3 position = new Vector3(point.x, mapGenerator.GetHeightAt(pointData.height), point.y);
                GameObject gameObj = Instantiate(obj.obj, position, Quaternion.Euler(0f, (float)prng.NextDouble(), 0f));
                float scaleMultiplier = (float)prng.NextDouble() * (obj.scaleFactor * 2) - obj.scaleFactor;
                gameObj.transform.localScale += Vector3.one * scaleMultiplier;
                SetParent(gameObj);
            }
        }
    }

    void SetParent(GameObject obj)
    {
        //float width = mapGenerator.mapSizeX == 1 ? mapGenerator.chunkSize - MapGenerator.borderSize * 2 : mapGenerator.chunkSize - MapGenerator.borderShift;
        //float height = mapGenerator.mapSizeY == 1 ? mapGenerator.chunkSize - MapGenerator.borderSize * 2 : mapGenerator.chunkSize - MapGenerator.borderShift;
        float width = mapGenerator.chunkSize - (float)(MapGenerator.borderSize * 2) / LOD;
        float height = mapGenerator.chunkSize - (float)(MapGenerator.borderSize * 2) / LOD;
        int x = (int)(obj.transform.position.x / width);
        int y = (int)(obj.transform.position.z / height);
        Vector2 coord = new Vector2(x, y);
        obj.transform.SetParent(MapGenerator.terrainDataDictionary[coord].GetGameObject().transform);
    }

    PointData CalculatePointData(float[,] heightMap, float cellSize, Vector2 point)
    {
        float xOffset = Mathf.InverseLerp(0f, cellSize, (point.x - (int)point.x) % cellSize);
        float yOffset = Mathf.InverseLerp(0f, cellSize, (point.y - (int)point.y) % cellSize);
        int x = (int)(point.x * LOD);
        int y = (int)(point.y * LOD);

        float p00 = heightMap[x, y];
        float p10 = heightMap[x + 1, y];
        float p01 = heightMap[x, y + 1];
        float p11 = heightMap[x + 1, y + 1];

        float xLerp1 = Mathf.Lerp(p00, p10, xOffset);
        float xLerp2 = Mathf.Lerp(p01, p11, xOffset);
        float height = Mathf.Lerp(xLerp1, xLerp2, yOffset);

        float gradientX = (p00 - p10) * yOffset + (p01 - p11) * (1 - yOffset);
        float gradientY = (p00 - p01) * xOffset + (p10 - p11) * (1 - xOffset);
        Vector2 gradient = new Vector2(gradientX, gradientY);

        PointData pointData = new PointData(height, gradient);

        return pointData;
    }

    class PointData
    {
        public float height;
        public Vector2 gradient;

        public PointData(float height, Vector2 gradient)
        {
            this.height = height;
            this.gradient = gradient;
        }
    }
}

[Serializable]
public class TerrainObjectData
{
    public GameObject obj;
    public float spawnRadius = 1f;
    public int spawnIterationCount = 10;
    public float minSpawnHeight = 0.2f;
    public float maxSpawnHeight = 0.8f;
    public float spawnHeightOffset = 0f;
    [Range(0f, 0.1f)]
    public float maxSpawnSlopeValue;
    [Range(0f, 0.99f)]
    public float scaleFactor = 0.25f;
}
