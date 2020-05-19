using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainObjectManager : MonoBehaviour
{

    public List<TerrainObject> objects;

    int LOD;
    MapGenerator mapGenerator;

    public void GenerateObjects(float[,] heightMap, MapGenerator mapGenerator)
    {
        this.mapGenerator = mapGenerator;
        int chunkCount = mapGenerator.mapSizeX * mapGenerator.mapSizeY;
        LOD = mapGenerator.levelOfDetail == 0 ? 1 : mapGenerator.levelOfDetail * 2;

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Dictionary<int, List<Vector2>> objectsMap = new Dictionary<int, List<Vector2>>();

        for (int i = 0; i < objects.Count; i++) {
            objectsMap.Add(i, PoissonDiskSampling.GeneratePoints(mapGenerator.seed, width, height, chunkCount, objects[i].spawnRadius, objects[i].spawnIterationCount));
            SpawnObjects(objectsMap[i], objects[i], heightMap);
        }
    }

    void SpawnObjects(List<Vector2> points, TerrainObject obj, float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float cellSize = 1f / LOD;

        foreach (Vector2 point in points) {
            float currentHeight = CalculatePointHeight(heightMap, cellSize, point);
            if (currentHeight >= obj.minSpawnHeight && currentHeight <= obj.maxSpawnHeight) {
                Vector3 position = new Vector3(point.x, mapGenerator.GetHeightAt(currentHeight), point.y);
                GameObject gameObj = Instantiate(obj.obj, position, Quaternion.Euler(0f, Random.value, 0f));
                SetParent(gameObj);
            }
        }
    }

    void SetParent(GameObject obj)
    {
        //float width = mapGenerator.mapSizeX == 1 ? mapGenerator.chunkSize - MapGenerator.borderSize * 2 : mapGenerator.chunkSize - MapGenerator.borderShift;
        //float height = mapGenerator.mapSizeY == 1 ? mapGenerator.chunkSize - MapGenerator.borderSize * 2 : mapGenerator.chunkSize - MapGenerator.borderShift;
        float width = mapGenerator.chunkSize - MapGenerator.borderSize * 2;
        float height = mapGenerator.chunkSize - MapGenerator.borderSize * 2;
        int x = (int)(obj.transform.position.x / width);
        int y = (int)(obj.transform.position.z / height);
        Vector2 coord = new Vector2(x, y);
        try {
            obj.transform.SetParent(MapGenerator.terrainDataDictionary[coord].GetGameObject().transform);
        } catch (KeyNotFoundException e) {
            string s = "Keys:";
            foreach (KeyValuePair<Vector2, TerrainData> kvp in MapGenerator.terrainDataDictionary) {
                s += "\n" + kvp.Key.ToString();
            }
            Debug.Log(s);
            Debug.Log("coord: " + coord);
        }
    }

    float CalculatePointHeight(float[,] heightMap, float cellSize, Vector2 point)
    {
        float xOffset = Mathf.InverseLerp(0f, cellSize, (point.x - (int)point.x) % cellSize);
        float yOffset = Mathf.InverseLerp(0f, cellSize, (point.y - (int)point.y) % cellSize);
        int x = (int)point.x * LOD;
        int y = (int)point.y * LOD;

        float p00 = heightMap[x, y];
        float p10 = heightMap[x + 1, y];
        float p01 = heightMap[x, y + 1];
        float p11 = heightMap[x + 1, y + 1];

        float xLerp1 = Mathf.Lerp(p00, p01, xOffset);
        float xLerp2 = Mathf.Lerp(p10, p11, xOffset);
        float height = Mathf.Lerp(xLerp1, xLerp2, yOffset);
        //float height = p00 * (1 - xOffset) * (1 - yOffset) + p01 * xOffset * (1 - yOffset) + p10 * (1 - xOffset) * yOffset + p11 * xOffset * yOffset;

        return height;
    }

    [System.Serializable]
    public class TerrainObject
    {
        public GameObject obj;
        public float spawnRadius = 1f;
        public int spawnIterationCount = 10;
        public float minSpawnHeight = 0.2f;
        public float maxSpawnHeight = 0.8f;
        public float spawnHeightOffset = 0f;
    }
}
