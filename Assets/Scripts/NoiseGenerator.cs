using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class NoiseGenerator
{

    static int seed;
    static float maxNoiseValue;
    static float minNoiseValue;

    public static float[,] GenerateNoiseMap(int width, int height, int LOD, NoiseData nd, Vector2 offset) 
    {
        System.Random prng = new System.Random(seed);
        float[,] noiseMap = new float[width * LOD, height * LOD];
        width *= LOD;
        height *= LOD;
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        Vector2[] octaveOffsets = new Vector2[nd.octaves];
        for (int i = 0; i < nd.octaves; i++) {
            float x = prng.Next(-10000, 10000) + offset.x;
            float y = prng.Next(-10000, 10000) + offset.y;
            octaveOffsets[i] = new Vector2(x, y);
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                float value = 0;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < nd.octaves; i++) {
                    float tX = ((x - halfWidth) / LOD + octaveOffsets[i].x) / nd.scale * frequency;
                    float tY = ((y  - halfHeight) / LOD + octaveOffsets[i].y) / nd.scale * frequency;
                    value += Mathf.PerlinNoise(tX, tY) * amplitude;

                    amplitude *= nd.persistance;
                    frequency *= nd.lacunarity;
                }
                
                noiseMap[x, y] = value;

                if (value > maxNoiseValue) {
                    maxNoiseValue = value;
                }
                if (value < minNoiseValue) {
                    minNoiseValue = value;
                }
            }
        }

        return noiseMap;
    }

    public static float[,] NormilazeNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }

    static public void SetSeed(int newSeed)
    {
        seed = newSeed;
    }

    public static void ClearMaxMinValue()
    {
        maxNoiseValue = float.MinValue;
        minNoiseValue = float.MaxValue;
    }

    #region NoiseFilters

    public static float[,] CascadeNoiseFilter2(float [,] noiseMap, int cascadeCount, float heightMultiplier, float intensity)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float value = Mathf.InverseLerp(0f, heightMultiplier, heightMultiplier / cascadeCount);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                for (int i = 0; i <= cascadeCount; i++) {
                    float cascade = value * i;
                    if (Mathf.Abs(cascade - noiseMap[x, y]) < (value * 0.5f)) {
                        //float temp = value - (Mathf.Abs(cascade - noiseMap[x, y]));
                        //temp = Mathf.InverseLerp(0f, value, value - temp);
                        noiseMap[x, y] = Mathf.Lerp(noiseMap[x, y], cascade, intensity);
                        break;
                    }
                }                
            }
        }

        return noiseMap;
    }

    public static float[,] CascadeNoiseFilter(float[,] noiseMap, int cascadeCount, float heightMultiplier, float intensity)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float value = Mathf.InverseLerp(0f, heightMultiplier, heightMultiplier / cascadeCount);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                for (int i = 0; i <= cascadeCount; i++) {
                    float cascade = value * i;
                    if (noiseMap[x, y] < cascade) {
                        noiseMap[x, y] = Mathf.Lerp(noiseMap[x, y], cascade, intensity);
                        break;
                    }
                }
            }
        }

        return noiseMap;
    }

    #endregion

}
