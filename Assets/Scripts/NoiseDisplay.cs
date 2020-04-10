using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseDisplay : MonoBehaviour
{
    public GameObject noiseDisplay;
    public GameObject chunkPrefab;
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

    private Texture2D GenerateColorTexture(float[,] noiseMap, MapGenerator.TerrainType[] regions)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                for (int i = 0; i < regions.Length; i++) {
                    if (noiseMap[x, y] >= regions[i].height) {
                        texture.SetPixel(x, y, regions[i].color);
                    } else {
                        break;
                    }
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }

    public void DrawColorMap(float[,] noiseMap, MapGenerator.TerrainType[] regions)
    {
        noiseDisplay.SetActive(true);

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = GenerateColorTexture(noiseMap, regions);
        noiseDisplay.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    public GameObject DrawTerrainChunkWithColorMap(Mesh terrainMesh, float[,] noiseMap, Vector2 coord, MapGenerator.TerrainType[] regions)
    {
        if (noiseDisplay.activeInHierarchy) 
            noiseDisplay.SetActive(false);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(coord.x, 0f, coord.y), Quaternion.identity);
        Texture2D texture = GenerateColorTexture(noiseMap, regions);

        chunk.name = "Chunk " + coord.ToString();
        chunk.GetComponent<MeshFilter>().mesh = terrainMesh;
        chunk.GetComponent<Renderer>().material.mainTexture = texture;

        return chunk;
    }

    public GameObject DrawTerrainChunkMeshWithCustomMaterial(Mesh terrainMesh, float[,] noiseMap, Vector2 coord)
    {
        if (noiseDisplay.activeInHierarchy)
            noiseDisplay.SetActive(false);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(coord.x, 0f, coord.y) , Quaternion.identity);
        chunk.name = "Chunk " + coord.ToString();
        chunk.GetComponent<MeshFilter>().sharedMesh = terrainMesh;

        return chunk;
    }
}
