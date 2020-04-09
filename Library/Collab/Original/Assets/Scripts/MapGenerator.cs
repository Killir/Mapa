using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawType { heightMap, colorMap, terrain, terrain_colorMap}

    [Header("Map parameters")]
    [Range(0,10)]
    public int mapSizeX;
    [Range(0, 10)]
    public int mapSizeZ;
    public int chunkSize;
    [Range(0, 5)]
    public int levelOfDetail;
    public TerrainType[] regions;

    [Header("Global noise parameters")]
    public int seed;

    [Space(20)]
    public bool cascadeNoiseFilter;
    public int cascadeCount;
    [Range(0f,1f)]
    public float CNFintensity;

    [Space(10)]
    public bool smoothNoiseFilter;
    public int checkPointCount;
    [Range(0f, 1f)]
    public float SNFintensity;

    [Header("First noise parameters")]
    public NoiseData mainNoiseData;
    public float heightMultiplier;

    [Header("Second noise parameters")]
    public NoiseData secondNoiseData;
    public bool useSecondNoiseMap;
    [Range(0f, 1f)]
    public float influencePower;

    [Header("Noise Display parameters")]
    public DrawType drawMode;
    public bool autoUpdate;


    private float[,] SetNoiseFilters(float [,] noiseMap)
    {
        if (cascadeNoiseFilter) {
            NoiseGenerator.CascadeNoiseFilter(noiseMap, cascadeCount, heightMultiplier, CNFintensity);
        }
        if (smoothNoiseFilter) {
            NoiseGenerator.SmoothNoiseFilter(noiseMap, checkPointCount, SNFintensity);
        }

        return noiseMap;
    }

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(seed, chunkSize, chunkSize, levelOfDetail, mainNoiseData.scale, mainNoiseData.octaves, mainNoiseData.persistance, mainNoiseData.lacunarity, mainNoiseData.offset, true);

        float[,] secondNoiseMap = null, combinedNoiseMap = null;
        if (useSecondNoiseMap) {
            secondNoiseMap = NoiseGenerator.GenerateNoiseMap(seed, chunkSize, chunkSize, levelOfDetail, secondNoiseData.scale, secondNoiseData.octaves, secondNoiseData.persistance, secondNoiseData.lacunarity, secondNoiseData.offset, false);
            combinedNoiseMap = NoiseGenerator.CombineNoiseMaps(noiseMap, secondNoiseMap, influencePower);
        }

        noiseMap = SetNoiseFilters(noiseMap);

        switch (drawMode) {
            case DrawType.heightMap:
                if (useSecondNoiseMap) {
                    FindObjectOfType<MapDisplay>().DrawHeightMap(combinedNoiseMap);
                } else {
                    FindObjectOfType<MapDisplay>().DrawHeightMap(noiseMap);
                }
                break;
            case DrawType.colorMap:
                if (useSecondNoiseMap) {
                    FindObjectOfType<MapDisplay>().DrawColorMap(combinedNoiseMap, regions);
                } else {
                    FindObjectOfType<MapDisplay>().DrawColorMap(noiseMap, regions);
                }
                break;
            case DrawType.terrain:
                FindObjectOfType<MapDisplay>().DrawMesh(TerrainMeshGenerator.CreateTerrain(noiseMap, heightMultiplier, secondNoiseMap, influencePower, levelOfDetail));
                break;
            case DrawType.terrain_colorMap:
                FindObjectOfType<MapDisplay>().DrawMeshWithColorMap(TerrainMeshGenerator.CreateTerrain(noiseMap, heightMultiplier, secondNoiseMap, influencePower, levelOfDetail), noiseMap, regions);
                break;
            
        }
    
    }

    public void SetNewSeed()
    {
        seed = Random.Range(0, 100000);
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
        if (checkPointCount < 1) {
            checkPointCount = 1;
        }
    }

}
