using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawType { heightMap, colorMap, terrain_customMaterial, terrain_colorMap}
    const int borderSize = 1;

    [Header ("Map parameters")]
    [Range(1,10)]
    public int mapSizeX;
    [Range(1, 10)]
    public int mapSizeZ;
    public int chunkSize;
    [Range(0, 5)]
    public int levelOfDetail;
    public TerrainType[] regions;

    [Header("Display parameters")]
    public AnimationCurve heightCurve;

    [Header ("Global noise parameters")]
    public int seed;

    [Space(20)]
    public bool cascadeNoiseFilter;
    public int cascadeCount;
    [Range(0f,1f)]
    public float CNFintensity;

    [Header ("Height noise parameters")]
    public NoiseData heightNoiseData;
    public float heightMultiplier;

    [Header ("Noise Display parameters")]
    public DrawType drawMode;
    public bool autoUpdate;

    static Dictionary<Vector2, TerrainData> terrainDataDictionary = new Dictionary<Vector2, TerrainData>();
    int LOD;

    private float[,] SetNoiseFilters(float [,] noiseMap)
    {
        if (cascadeNoiseFilter) {
            noiseMap = NoiseGenerator.CascadeNoiseFilter(noiseMap, cascadeCount, heightMultiplier, CNFintensity);
        }
        return noiseMap;
    }

    float[,] GetNoiseMap(NoiseData noiseData, Vector2 coord)
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize , chunkSize, LOD, noiseData, coord);
        return noiseMap;
    }

    GameObject GenerateChunk(float[,] noiseMap, Vector2 coord)
    {
        GameObject chunk = null;
        if (drawMode == DrawType.terrain_customMaterial) {
            chunk = FindObjectOfType<NoiseDisplay>().DrawTerrainChunkMeshWithCustomMaterial(TerrainMeshGenerator.CreateTerrain(noiseMap, heightMultiplier, heightCurve, LOD), noiseMap, coord);
        } else if (drawMode == DrawType.terrain_colorMap) {
            chunk = FindObjectOfType<NoiseDisplay>().DrawTerrainChunkWithColorMap(TerrainMeshGenerator.CreateTerrain(noiseMap, heightMultiplier, heightCurve, LOD), noiseMap, coord, regions);
        }

        return chunk;
    }

    void SetTerrainMap()
    {
        Dictionary<Vector2, float[,]> noiseMapDictionary = new Dictionary<Vector2, float[,]>();
        List<Vector2> coordList = new List<Vector2>();
        float borderShift = (float)(borderSize * 3) / LOD; // почему 3? хз!
        for (int z = 0; z < mapSizeZ; z++) {
            for (int x = 0; x < mapSizeX; x++) {
                Vector2 coord = new Vector2(x * (chunkSize - borderShift), z * (chunkSize - borderShift));
                coordList.Add(coord);
                noiseMapDictionary.Add(coord, GetNoiseMap(heightNoiseData, coord));
            }
        }

        foreach(Vector2 v in coordList) {
            noiseMapDictionary[v] = NoiseGenerator.NormilazeNoiseMap(noiseMapDictionary[v]);
            noiseMapDictionary[v] = SetNoiseFilters(noiseMapDictionary[v]);
            terrainDataDictionary.Add(v, new TerrainData(GenerateChunk(noiseMapDictionary[v], v), v));
        }

    }

    public void ClearTerrainDictionary()
    {        
        foreach (KeyValuePair<Vector2, TerrainData> kvp in terrainDataDictionary) {
            DestroyImmediate(kvp.Value.terrainGameObject.gameObject);
        }
        terrainDataDictionary.Clear();
        NoiseGenerator.ClearMaxMinValue();
    }

    public void GenerateMap()
    {
        ClearTerrainDictionary();

        LOD = levelOfDetail == 0 ? 1 : levelOfDetail * 2;

        switch (drawMode) {
            case DrawType.heightMap:
                float[,] noiseMap = GetNoiseMap(heightNoiseData, Vector2.zero);
                noiseMap = NoiseGenerator.NormilazeNoiseMap(noiseMap);
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(noiseMap);
                break;
            case DrawType.colorMap:
                noiseMap = GetNoiseMap(heightNoiseData, Vector2.zero);
                noiseMap = NoiseGenerator.NormilazeNoiseMap(noiseMap);
                FindObjectOfType<NoiseDisplay>().DrawColorMap(noiseMap, regions);
                break;
            default:
                SetTerrainMap();
                break;

        }
    }

    public void SetNewSeed()
    {
        seed = UnityEngine.Random.Range(0, 100000);
        NoiseGenerator.SetSeed(seed);
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
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
    public GameObject terrainGameObject;
    public Vector2 coord;

    public TerrainData(GameObject terrainGameObject, Vector2 coord)
    {
        this.terrainGameObject = terrainGameObject;
        this.coord = coord;
    }
}
