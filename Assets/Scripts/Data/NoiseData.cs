using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject
{

    public float scale;
    public int octaves;
    [Range(0f, 1f)]
    public float persistance;
    public float lacunarity;
    [Range(0, 7)]
    public int noiseIndex;

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
    }

}
