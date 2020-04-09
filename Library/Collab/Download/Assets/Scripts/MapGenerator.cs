using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawType { heightMap, colorMap, terrain}

    const int chunkSize = 241;

    [Header("Map parameters")]
    [Range(0, 6)]
    public int LOD;
    [Range(0, 5)]
    public int LevelOfDetail;
    public TerrainType[] regions;

    [Header("Noise parameters")]
    public int seed;
    public bool newSeed = false;
    public float scale;
    public int octaves;
    [Range(0f, 1f)]
    public float persistance;
    public float lacunarity;
    public Vector2 offset;

    [Header("Terrain parameters")]
    public float heightMultiplier;
    public AnimationCurve meshHeightCurve;

    [Header("Noise Display parameters")]
    public DrawType drawMode;
    public bool autoUpdate;

    public void GenerateMap()
    {

        if (newSeed) {
            seed = Random.Range(0, 100000);
        }

        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(seed, chunkSize, chunkSize, scale, octaves, persistance, lacunarity, offset);

        switch (drawMode) {
            case DrawType.heightMap:
                FindObjectOfType<NoiseDisplay>().DrawHeightMap(noiseMap);
                break;
            case DrawType.colorMap:
                FindObjectOfType<NoiseDisplay>().DrawColorMap(noiseMap, regions);
                break;
            case DrawType.terrain:
                FindObjectOfType<NoiseDisplay>().DrawMesh(TerrainMeshGenerator.CreateTerrain(noiseMap, heightMultiplier, meshHeightCurve, LOD), noiseMap, regions);
                break;
        }
    
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
        if (scale <= 0) {
            scale = 0.001f;
        }

        if (lacunarity < 1) {
            lacunarity = 1;
        }

        if (octaves < 1) {
            octaves = 1;
        }

        if (heightMultiplier < 0) {
            heightMultiplier = 0;
        }
    }

}
