using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawType { heightMap, humidityMap, colorMap, terrain_customMaterial, terrain_colorMap }
    const int borderSize = 1;

    [Header ("Map parameters")]
    [Range(1,10)]
    public int mapSizeX;
    [Range(1, 10)]
    public int mapSizeY;
    public int chunkSize;
    [Range(0, 5)]
    public int levelOfDetail;

    [Header("Display parameters")]
    public AnimationCurve heightCurve;

    [Header ("Global noise parameters")]
    public int seed;
    public bool erode;
    public Erosion erosion;

    [Space(20)]
    public bool cascadeNoiseFilter;
    public int cascadeCount;
    [Range(0f,1f)]
    public float CNFintensity;

    [Header ("Height noise parameters")]
    public NoiseData heightNoiseData;
    public float heightMultiplier;

    [Header("Humidity noise parameters")]
    public NoiseData humidityNoiseData;


    [Header ("Noise Display parameters")]
    public DrawType drawMode;
    public bool autoUpdate;

    public static float borderShift = 0f;
    static Dictionary<Vector2, TerrainData> terrainDataDictionary = new Dictionary<Vector2, TerrainData>();
    int LOD;

    private void SetNoiseFilters(Dictionary<Vector2, float[,]> noiseMapDictionary, List<Vector2> coordList)
    {
        if (erode) {
            float[,] sharedHeightMap = CombineNoiseMaps(noiseMapDictionary, coordList);
            sharedHeightMap = erosion.Erode(seed, sharedHeightMap);
            noiseMapDictionary = SeparateNoiseMap(sharedHeightMap, noiseMapDictionary, coordList);
        }

        foreach (Vector2 v in coordList) {
            if (cascadeNoiseFilter) {
                noiseMapDictionary[v] = NoiseGenerator.CascadeNoiseFilter(noiseMapDictionary[v], cascadeCount, heightMultiplier, CNFintensity);
            }
        }
    }
    private void SetNoiseFilters(float[,] noiseMap, Vector2 coord)
    {
        if (erode) {
            noiseMap = erosion.Erode(seed, noiseMap);
        }

        if (cascadeNoiseFilter) {
            noiseMap = NoiseGenerator.CascadeNoiseFilter(noiseMap, cascadeCount, heightMultiplier, CNFintensity);
        }
        
    }

    float[,] CombineNoiseMaps(Dictionary<Vector2, float[,]> noiseMapDictionary, List<Vector2> coordList)
    {
        int width = mapSizeX * (chunkSize - (borderSize * 3)) + (borderSize * 3);
        int height = mapSizeY * (chunkSize - (borderSize * 3)) + (borderSize * 3);
        float[,] noiseMap = new float[width, height];

        foreach(Vector2 coord in coordList) {

            for (int j = 0; j < chunkSize; j++) {
                for (int i = 0; i < chunkSize; i++) {
                    int x = (int)coord.x * (chunkSize - (borderSize * 3)) + i;
                    int y = (int)coord.y * (chunkSize - (borderSize * 3)) + j;
                    noiseMap[x, y] = noiseMapDictionary[coord][i, j];
                }
            }
        }

        return noiseMap;
    }

    Dictionary<Vector2, float[,]> SeparateNoiseMap(float[,] noiseMap, Dictionary<Vector2, float[,]> noiseMapDictionary, List<Vector2> coordList)
    {

        foreach (Vector2 coord in coordList) {
            float[,] currentNoiseMap = new float[chunkSize, chunkSize];

            for (int j = 0; j < chunkSize; j++) {
                for (int i = 0; i < chunkSize; i++) {
                    int x = (int)coord.x * (chunkSize - (borderSize * 3)) + i;
                    int y = (int)coord.y * (chunkSize - (borderSize * 3)) + j;
                    currentNoiseMap[i, j] = noiseMap[x, y];
                }
            }
            noiseMapDictionary[coord] = currentNoiseMap;
        }

        return noiseMapDictionary;
    }

    float[,] GetNoiseMap(NoiseData noiseData, Vector2 coord)
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize , chunkSize, LOD, noiseData, coord);
        return noiseMap;
    }

    GameObject GenerateChunk(float[,] heightMap, float[,] humidityMap, Vector2 coord)
    {
        GameObject chunk = null;
        Mesh terrainMesh = TerrainMeshGenerator.CreateTerrain(heightMap, heightMultiplier, heightCurve, LOD);

        if (drawMode == DrawType.terrain_customMaterial) {
            chunk = FindObjectOfType<NoiseDisplay>().DrawTerrainChunkWithCustomMaterial(terrainMesh, heightMap, humidityMap, coord);
        } else if (drawMode == DrawType.terrain_colorMap) {
            chunk = FindObjectOfType<NoiseDisplay>().DrawTerrainChunkWithColorMap(terrainMesh, heightMap, humidityMap, coord);
        }

        return chunk;
    }

    void SetTerrainMap()
    {
        Dictionary<Vector2, float[,]> heightMapDictionary = new Dictionary<Vector2, float[,]>();
        Dictionary<Vector2, float[,]> humidityMapDictionary = new Dictionary<Vector2, float[,]>();
        List<Vector2> coordList = new List<Vector2>();

        for (int z = 0; z < mapSizeY; z++) {
            for (int x = 0; x < mapSizeX; x++) {            
                Vector2 simpleCoord = new Vector2(x, z);
                Vector2 realCoord = simpleCoord * (chunkSize - borderShift);
                coordList.Add(simpleCoord);

                heightMapDictionary.Add(simpleCoord, GetNoiseMap(heightNoiseData, realCoord));
                humidityMapDictionary.Add(simpleCoord, GetNoiseMap(humidityNoiseData, realCoord));
            }
        }

        foreach (Vector2 v in coordList) {
            heightMapDictionary[v] = NoiseGenerator.NormilazeNoiseMap(heightMapDictionary[v], heightNoiseData.noiseIndex);
            humidityMapDictionary[v] = NoiseGenerator.NormilazeNoiseMap(humidityMapDictionary[v], humidityNoiseData.noiseIndex);
        }

        SetNoiseFilters(heightMapDictionary, coordList);

        foreach (Vector2 v in coordList) {
            GameObject terrainGameObject = GenerateChunk(heightMapDictionary[v], humidityMapDictionary[v], v * (chunkSize - borderShift));
            terrainDataDictionary.Add(v, new TerrainData(terrainGameObject, heightMapDictionary[v], humidityMapDictionary[v]));
        }

    }

    public static float[,] GetNeighborChunkHeightMap(Vector2 simpleCoord, Vector2 step)
    {
        simpleCoord += step;
        if (terrainDataDictionary.ContainsKey(simpleCoord))
            return terrainDataDictionary[simpleCoord].GetHeightMap();
        else 
            return null;
    } 

    public void ClearTerrainDictionary()
    {        
        foreach (KeyValuePair<Vector2, TerrainData> kvp in terrainDataDictionary) {
            DestroyImmediate(kvp.Value.GetGameObject());
        }
        terrainDataDictionary.Clear();
        NoiseGenerator.ClearMaxMinValue();
    }

    public void GenerateMap()
    {
        ClearTerrainDictionary();

        LOD = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        borderShift = (float)(borderSize * 3) / LOD; // почему 3? хз!

        switch (drawMode) {
            case DrawType.heightMap:
                float[,] noiseMap = GetNoiseMap(heightNoiseData, Vector2.zero);
                noiseMap = NoiseGenerator.NormilazeNoiseMap(noiseMap, heightNoiseData.noiseIndex);
                SetNoiseFilters(noiseMap, Vector2.zero);
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(noiseMap);
                break;
            case DrawType.humidityMap:
                noiseMap = GetNoiseMap(humidityNoiseData, Vector2.zero);
                noiseMap = NoiseGenerator.NormilazeNoiseMap(noiseMap, humidityNoiseData.noiseIndex);
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(noiseMap);
                break;
            case DrawType.colorMap:
                noiseMap = GetNoiseMap(heightNoiseData, Vector2.zero);
                noiseMap = NoiseGenerator.NormilazeNoiseMap(noiseMap, heightNoiseData.noiseIndex);
                SetNoiseFilters(noiseMap, Vector2.zero);
                float[,] humidityMap = GetNoiseMap(humidityNoiseData, Vector2.zero);
                FindObjectOfType<NoiseDisplay>().DrawColorMap(noiseMap, humidityMap);
                break;
            default:
                SetTerrainMap();
                break;

        }
    }

    public void SetNewSeed()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        NoiseGenerator.SetSeed(seed);
    }

    private void OnValidate()
    { 
        if (heightMultiplier < 0) {
            heightMultiplier = 0;
        }
        if (cascadeCount < 1) {
            cascadeCount = 1;
        }
    }

}

public class TerrainData
{
    GameObject terrainGameObject;
    float[,] heightMap;
    float[,] humidityHap;

    public TerrainData(GameObject terrainGameObject, float[,] heightMap, float[,] humidityHap)
    {
        this.terrainGameObject = terrainGameObject;
        this.heightMap = heightMap;
        this.humidityHap = humidityHap;
    }

    public GameObject GetGameObject()
    {
        return terrainGameObject;
    }

    public float[,] GetHeightMap()
    {
        return heightMap;
    }

    public float[,] GetHumidityMap()
    {
        return humidityHap;
    }

}
