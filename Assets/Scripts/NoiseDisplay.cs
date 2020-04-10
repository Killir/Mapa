using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseDisplay : MonoBehaviour
{
    public GameObject noiseDisplay;
    public GameObject chunkPrefab;
    public RegionMap regionMap;
    public Material oneTextureMaterial;

    public void DrawHeightMap(float[,] noiseMap)
    {
        noiseDisplay.SetActive(true);

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x <width; x++) {
                texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, noiseMap[x, y]));
            }
        }

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        noiseDisplay.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    private Texture2D GenerateColorTexture(float[,] noiseMap, float[,] humidityMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                /*int heightIndex, humadityIndex;
                for (int i = 0; i < regions.Length; i++) {
                    if (noiseMap[x, y] >= regions[i].height) {
                        heightIndex = i;
                    }
                    if (humidityMap[x, y] >= regions[i].height) {
                        humadityIndex = i;
                    }
                }*/

                //texture.SetPixel(x, y, regions[index].color);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }

    public void DrawColorMap(float[,] noiseMap, float[,] humidityMap)
    {
        noiseDisplay.SetActive(true);

        Texture2D texture = GenerateColorTexture(noiseMap, humidityMap);
        noiseDisplay.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    public GameObject DrawTerrainChunkWithColorMap(Mesh terrainMesh, float[,] noiseMap, float[,] humidityMap, Vector2 coord)
    {
        if (noiseDisplay.activeInHierarchy) 
            noiseDisplay.SetActive(false);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(coord.x, 0f, coord.y), Quaternion.identity);
        Texture2D texture = GenerateColorTexture(noiseMap, humidityMap);

        chunk.name = "Chunk " + coord.ToString();
        chunk.GetComponent<MeshFilter>().mesh = terrainMesh;
        chunk.GetComponent<Renderer>().material.mainTexture = texture;

        return chunk;
    }

    public GameObject DrawTerrainChunkWithCustomMaterial(Mesh terrainMesh, float[,] noiseMap, float[,] humidityMap, Vector2 coord)
    {
        if (noiseDisplay.activeInHierarchy)
            noiseDisplay.SetActive(false);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(coord.x, 0f, coord.y) , Quaternion.identity);
        chunk.name = "Chunk " + coord.ToString();
        chunk.GetComponent<MeshFilter>().sharedMesh = terrainMesh;

        return chunk;
    }
}
