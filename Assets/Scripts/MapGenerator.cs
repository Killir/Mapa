using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawType { heightMap, humidityMap, colorMap, terrain_customMaterial, terrain_colorMap }
    public const int borderSize = 1;

    [Header ("Map parameters")]
    [Range(1,10)]
    public int mapSizeX;
    [Range(1, 10)]
    public int mapSizeY;
    public int chunkSize;
    [Range(0, 5)]
    public int levelOfDetail;
    public bool generateObjects;
    public TerrainObjectManager terrainObjectManager;

    [Header("Mesh parameters")]
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


    [Header("Noise Display parameters")]
    public NoiseDisplay noiseDisplay;
    public DrawType drawMode;
    public bool autoUpdate;

    public static float borderShift = 0f;
    public static int LOD = 1;
    public static Dictionary<Vector2, TerrainData> terrainDataDictionary = new Dictionary<Vector2, TerrainData>();

    float[,] sharedHeightMap = null;

    private void Start()
    {
        GenerateMap();
    }

    private void SetNoiseFilters(Dictionary<Vector2, float[,]> noiseMapDictionary, int noiseIndex, List<Vector2> coordList)
    {
        if (sharedHeightMap == null) {
            sharedHeightMap = CombineNoiseMaps(noiseMapDictionary, coordList);
        }

        if (erode) {
            sharedHeightMap = erosion.Erode(seed, sharedHeightMap, noiseIndex);
            noiseMapDictionary = SeparateNoiseMap(sharedHeightMap, noiseMapDictionary, coordList);
        }

        foreach (Vector2 v in coordList) {
            if (cascadeNoiseFilter) {
                noiseMapDictionary[v] = NoiseGenerator.CascadeNoiseFilter(noiseMapDictionary[v], cascadeCount, heightMultiplier, CNFintensity);
            }
        }
    }
    private void SetNoiseFilters(float[,] noiseMap, int noiseIndex, Vector2 coord)
    {
        if (erode) {
            noiseMap = erosion.Erode(seed, noiseMap, noiseIndex);
        }

        if (cascadeNoiseFilter) {
            noiseMap = NoiseGenerator.CascadeNoiseFilter(noiseMap, cascadeCount, heightMultiplier, CNFintensity);
        }
        
    }

    float[,] CombineNoiseMaps(Dictionary<Vector2, float[,]> noiseMapDictionary, List<Vector2> coordList)
    {
        int mapWidth = chunkSize * LOD;
        int mapHeight = chunkSize * LOD;
        int width = mapSizeX * (mapWidth - (borderSize * 3)) + (borderSize * 3);
        int height = mapSizeY * (mapHeight - (borderSize * 3)) + (borderSize * 3);

        float[,] noiseMap = new float[width, height];

        foreach(Vector2 coord in coordList) {

            for (int j = 0; j < mapWidth; j++) {
                for (int i = 0; i < mapHeight; i++) {
                    int x = (int)coord.x * (mapWidth - (borderSize * 3)) + i;
                    int y = (int)coord.y * (mapHeight - (borderSize * 3)) + j;
                    noiseMap[x, y] = noiseMapDictionary[coord][i, j];
                }
            }
        }

        return noiseMap;
    }

    Dictionary<Vector2, float[,]> SeparateNoiseMap(float[,] noiseMap, Dictionary<Vector2, float[,]> noiseMapDictionary, List<Vector2> coordList)
    {
        int mapWidth = chunkSize * LOD;
        int mapheight = chunkSize * LOD;
        foreach (Vector2 coord in coordList) {
            float[,] currentNoiseMap = new float[chunkSize * LOD, chunkSize * LOD];

            for (int j = 0; j < mapheight; j++) {
                for (int i = 0; i < mapWidth; i++) {
                    int x = (int)coord.x * (mapWidth - (borderSize * 3)) + i;
                    int y = (int)coord.y * (mapheight - (borderSize * 3)) + j;
                    currentNoiseMap[i, j] = noiseMap[x, y];
                }
            }
            noiseMapDictionary[coord] = currentNoiseMap;
        }

        return noiseMapDictionary;
    }

    float[,] GenerateNoiseMap(NoiseData noiseData, Vector2 coord)
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize , chunkSize, LOD, noiseData, coord);
        return noiseMap;
    }

    GameObject GenerateChunk(float[,] heightMap, float[,] humidityMap, Vector2 coord)
    {
        GameObject chunk = null;
        Mesh terrainMesh = TerrainMeshGenerator.CreateTerrain(heightMap, heightMultiplier, heightCurve, LOD);

        if (drawMode == DrawType.terrain_customMaterial) {
            chunk = noiseDisplay.DrawTerrainChunkWithCustomMaterial(terrainMesh, humidityMap, coord);
        } else if (drawMode == DrawType.terrain_colorMap) {
            chunk = noiseDisplay.DrawTerrainChunkWithColorMap(terrainMesh, heightMap, humidityMap, coord);
        }

        return chunk;
    }

    void SetTerrainMap()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Dictionary<Vector2, float[,]> heightMapDictionary = new Dictionary<Vector2, float[,]>();
        Dictionary<Vector2, float[,]> humidityMapDictionary = new Dictionary<Vector2, float[,]>();
        List<Vector2> coordList = new List<Vector2>();

        for (int z = 0; z < mapSizeY; z++) {
            for (int x = 0; x < mapSizeX; x++) {            
                Vector2 simpleCoord = new Vector2(x, z);
                Vector2 realCoord = simpleCoord * (chunkSize - borderShift);
                coordList.Add(simpleCoord);

                heightMapDictionary.Add(simpleCoord, GenerateNoiseMap(heightNoiseData, realCoord));
                humidityMapDictionary.Add(simpleCoord, GenerateNoiseMap(humidityNoiseData, realCoord));
            }
        }

        foreach (Vector2 v in coordList) {
            heightMapDictionary[v] = NoiseGenerator.NormilazeNoiseMap(heightMapDictionary[v], heightNoiseData.noiseIndex);
            humidityMapDictionary[v] = NoiseGenerator.NormilazeNoiseMap(humidityMapDictionary[v], humidityNoiseData.noiseIndex);
        }

        SetNoiseFilters(heightMapDictionary, heightNoiseData.noiseIndex, coordList);

        if (drawMode == DrawType.terrain_customMaterial) {
            SetShaderData(humidityMapDictionary, coordList);
        }

        foreach (Vector2 v in coordList) {
            GameObject terrainGameObject = GenerateChunk(heightMapDictionary[v], humidityMapDictionary[v], v * (chunkSize - borderShift));
            terrainDataDictionary.Add(v, new TerrainData(terrainGameObject, heightMapDictionary[v], humidityMapDictionary[v]));
        }

        if (generateObjects) {
            terrainObjectManager.GenerateObjects(sharedHeightMap, this);
        }

        sw.Stop();
        Debug.Log("Time: " + sw.ElapsedMilliseconds + " milliseconds");

    }

    void SetShaderData(Dictionary<Vector2, float[,]> humidityMapDictionary, List<Vector2> coordList)
    {
        float[,] sharedHumidityMap = CombineNoiseMaps(humidityMapDictionary, coordList);
        Texture2D hmTexture = noiseDisplay.GenerateNoiseMapTexture(sharedHumidityMap);
        ShaderData sd = new ShaderData(noiseDisplay.customMaterial, mapSizeX, mapSizeY, chunkSize, chunkSize, hmTexture, noiseDisplay.regionMap);
        sd.SetMaxMinHeights(heightNoiseData.noiseIndex, GetHeightAt(1), GetHeightAt(0));
        noiseDisplay.regionMap.SetShaderData(sd);
    }

    public float GetHeightAt(float value)
    {
        return heightMultiplier * heightCurve.Evaluate(value);
    }

    public void ClearTerrainDictionary()
    {        
        foreach (KeyValuePair<Vector2, TerrainData> kvp in terrainDataDictionary) {
            DestroyImmediate(kvp.Value.GetGameObject());
        }
        terrainDataDictionary.Clear();
        NoiseGenerator.ClearMaxMinValue();
        sharedHeightMap = null;
    }

    public void GenerateMap()
    {
        ClearTerrainDictionary();
        NoiseGenerator.SetSeed(seed);

        LOD = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        borderShift = (float)(borderSize * 3) / LOD;

        switch (drawMode) {
            case DrawType.heightMap:
                float[,] heightMap = GenerateNoiseMap(heightNoiseData, Vector2.zero);
                heightMap = NoiseGenerator.NormilazeNoiseMap(heightMap, heightNoiseData.noiseIndex);
                SetNoiseFilters(heightMap, heightNoiseData.noiseIndex, Vector2.zero);
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(heightMap);
                break;
            case DrawType.humidityMap:
                float[,] humidityMap = GenerateNoiseMap(humidityNoiseData, Vector2.zero);
                humidityMap = NoiseGenerator.NormilazeNoiseMap(humidityMap, humidityNoiseData.noiseIndex);
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(humidityMap);
                break;
            case DrawType.colorMap:
                heightMap = GenerateNoiseMap(heightNoiseData, Vector2.zero);
                heightMap = NoiseGenerator.NormilazeNoiseMap(heightMap, heightNoiseData.noiseIndex);
                SetNoiseFilters(heightMap, heightNoiseData.noiseIndex, Vector2.zero);
                humidityMap = GenerateNoiseMap(humidityNoiseData, Vector2.zero);
                FindObjectOfType<NoiseDisplay>().DrawColorMap(heightMap, humidityMap);
                break;
            default:
                SetTerrainMap();
                break;

        }
    }

    public void SetNewSeed()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
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
