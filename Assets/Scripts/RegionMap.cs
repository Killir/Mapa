﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RegionMap : MonoBehaviour
{

    [Range(0f, 0.5f)]
    public float biomsBlend;
    public List<HumidityData> humidityLevels = new List<HumidityData>();
    public List<RegionData> regions = new List<RegionData>();

    public int textureSize;
    public bool useTextures;

    ShaderData sd = null;

    public RegionData Evaluate(float height, float humidity)
    {
        SortRegionDataList();

        RegionData region = new RegionData();
        List<RegionData> currentHumidityLevelRegions = new List<RegionData>();

        int humidityLevelCount = -1;
        foreach(HumidityData hd in humidityLevels) {
            if (hd.isActive) {
                humidityLevelCount++;
            }
        }
        int humiditylevelIndex = Mathf.RoundToInt(Mathf.Lerp(0f, humidityLevelCount, humidity));

        foreach (HumidityData hd in humidityLevels) {
            if (hd.GetIndex() == humiditylevelIndex && hd.isActive) {
                foreach (IncludedRegion incReg in hd.includedRegions) {
                    currentHumidityLevelRegions.Add(incReg.GetRegion());
                }
                break;
            }
        }

        currentHumidityLevelRegions[0].height = 0f;

        foreach (RegionData rd in currentHumidityLevelRegions) {
            if (height >= rd.height) {
                region = rd;
            } else 
                break;
        }

        return region;
    }

    public void SetShaderData(ShaderData sd)
    {
        this.sd = sd;
        UpdateAndApplyShaderData();
    }

    public void UpdateAndApplyShaderData()
    {
        if (sd != null) {
            sd.SetRegionMapValues(this);
            sd.SetShaderValue();
        } else {
            Debug.Log("Shader data is empty");
        }
    }

    public int[] GetHumidityLevelsLenght(int[] lenghts)
    {
        int i = 0;
        foreach (HumidityData hd in humidityLevels) {
            if (hd.isActive) {
                lenghts[i] = hd.includedRegions.Count;
                i++;
            }
        }

        return lenghts;
    }

    public int[] GetAllIncludedRegions(int[] incRegs)
    {
        int i = 0;
        foreach(HumidityData hd in humidityLevels) {
            if (hd.isActive) {
                foreach (IncludedRegion incReg in hd.includedRegions) {
                    incRegs[i] = incReg.index;
                    i++;
                }
            }
        }

        return incRegs.ToArray();
    }

    public void UpdateIndices()
    {
        int i = 0;
        foreach(HumidityData hd in humidityLevels) {
            if (hd.isActive) {
                hd.SetIndex(i);
                i++;
            } else {
                hd.SetIndex(-1);
            }
        }
    }

    public void DeleteRegion(RegionData region)
    {
        foreach (HumidityData hd in humidityLevels) {
            for (int i = 0; i < hd.includedRegions.Count; i++) {
                if (hd.includedRegions[i].GetRegion().name == region.name) {
                    hd.includedRegions.Remove(hd.includedRegions[i]);
                }
            }
        }
        regions.Remove(region);
    }

    public void SwapRegionDown(int index)
    {
        RegionData temp = regions[index];
        regions[index] = regions[index + 1];
        regions[index + 1] = temp;

        List<IncludedRegion> upIncReg = new List<IncludedRegion>();
        List<IncludedRegion> downIncReg = new List<IncludedRegion>();
        foreach (HumidityData hd in humidityLevels) {
            foreach (IncludedRegion incReg in hd.includedRegions) {
                if (incReg.index == index) {
                    upIncReg.Add(incReg);
                }
                if (incReg.index == index + 1) {
                    downIncReg.Add(incReg);
                }
            }
        }

        foreach (IncludedRegion incReg in upIncReg) {
            incReg.index = index + 1;
            incReg.SetRegion(regions[index + 1]);
        }
        foreach (IncludedRegion incReg in downIncReg) {
            incReg.index = index;
            incReg.SetRegion(regions[index]);
        }

    }

    public void SwapRegionUp(int index)
    {
        RegionData temp = regions[index];
        regions[index] = regions[index - 1];
        regions[index - 1] = temp;

        List<IncludedRegion> upIncReg = new List<IncludedRegion>();
        List<IncludedRegion> downIncReg = new List<IncludedRegion>();
        foreach (HumidityData hd in humidityLevels) {
            foreach(IncludedRegion incReg in hd.includedRegions) {
                if (incReg.index == index) {
                    downIncReg.Add(incReg);
                }
                if (incReg.index == index - 1) {
                    upIncReg.Add(incReg);
                }
            } 
        }

        foreach(IncludedRegion incReg in downIncReg) { 
            incReg.index = index - 1;
            incReg.SetRegion(regions[index - 1]);
        }
        foreach (IncludedRegion incReg in upIncReg) {
            incReg.index = index;
            incReg.SetRegion(regions[index]);
        }

    }

    public void SwitchColorStrenghts(bool useTextures)
    {
        foreach (RegionData region in regions) {
            if (useTextures) {
                region.colorStrenght = 0f;
            } else {
                region.colorStrenght = 1f;
            }
        }        
    }

    public void SortRegionDataList()
    {
        foreach (HumidityData hd in humidityLevels) {
            List<RegionData> list = hd.includedRegions.Select(x => x.GetRegion()).ToList();
            List<int> indexList = hd.includedRegions.Select(x => x.index).ToList();
            for (int i = 0; i < list.Count; i++) {
                for (int j = 0; j < list.Count - (i + 1); j++) {
                    if (list[j].height > list[j + 1].height) {
                        RegionData temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;

                        int tempIndex = indexList[j];
                        indexList[j] = indexList[j + 1];
                        indexList[j + 1] = tempIndex;
                    }
                }
            }

            int ind = 0;
            foreach(IncludedRegion incReg in hd.includedRegions) {
                incReg.SetRegion(list[ind]);
                incReg.index = indexList[ind];
                ind++;
            }
        }
    }

    public void LogHumidityLevels()
    {
        foreach (HumidityData hd in humidityLevels) {
            string log = hd.name;
            log += "\n" + "Index: " + hd.GetIndex().ToString();
            log += "\n" + "Is active: " + hd.isActive;
            log += "\n" + "Included regions count: " + hd.includedRegions.Count.ToString();
            Debug.Log(log);
        }
    }

}

[System.Serializable]
public class HumidityData
{
    public string name;
    public bool isActive = true;
    int index;
    public List<IncludedRegion> includedRegions = new List<IncludedRegion>();

    public HumidityData(int index)
    {
        this.index = index;
    }

    public int GetIndex()
    {
        return index;
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }
}

[System.Serializable]
public class IncludedRegion
{
    RegionData region;
    public int index;

    public void SetRegion(RegionData region)
    {
        this.region = region;
    }

    public RegionData GetRegion()
    {
        return region;
    }

}

[System.Serializable]
public class RegionData
{
    public string name = "Choose region";
    public float height;
    public Texture2D mainTexture;
    public Texture2D slopeTexture;
    public float scale = 1f;
    public float colorStrenght = 1f;
    public Color mainColor = new Color(0f, 0f, 0f, 1f);
    public Color slopeColor = new Color(0f, 0f, 0f, 1f);
    public float slopeThreshold = 0.5f;
    public float slopeBlendAmount = 0.7f;
    public float regionBlendAmount = 0.25f;
}

public class ShaderData
{
    const int maxHDCount = 8;
    const int maxRegionCount = 128;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    Material material;

    int hdCount;
    int[] hdLenghts = new int[maxHDCount];
    int[] hdIncRegs = new int[maxRegionCount];

    bool useTextures;

    int textureSize;
    float biomsBlend;
    int mapWidth;
    int mapHeight;
    int chunkWidth;
    int chunkHeight;
    float maxHeight;
    float minHeight;

    Texture2D humidityMap;

    Texture2D[] mainTextures = new Texture2D[maxRegionCount];
    Texture2D[] slopeTextures = new Texture2D[maxRegionCount];
    float[] scales = new float[maxRegionCount];
    float[] colorStrenghts = new float[maxRegionCount];
    Color[] mainColors = new Color[maxRegionCount];
    Color[] slopeColors = new Color[maxRegionCount];
    float[] heights = new float[maxRegionCount];
    float[] slopeThresholds = new float[maxRegionCount];
    float[] slopeBlendAmounts = new float[maxRegionCount];
    float[] regionBlendAmounts = new float[maxRegionCount];

    public ShaderData(Material material, int mapWidth, int mapHeight, int chunkWidth, int chunkHeight, Texture2D humidityMap, RegionMap regionMap)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.chunkWidth = chunkWidth;
        this.chunkHeight = chunkHeight;
        this.material = material;
        this.humidityMap = humidityMap;
        useTextures = regionMap.useTextures;

        SetRegionMapValues(regionMap);
    }

    T[] GetSizedArray<T>(T[] unsizedArray, T[] array)
    {
        for (int i = 0; i < unsizedArray.Length; i++) {
            array[i] = unsizedArray[i];
        }
        return array;
    }

    public void SetRegionMapValues(RegionMap regionMap)
    {
        textureSize = regionMap.textureSize;
        hdCount = 0;
        foreach (HumidityData hd in regionMap.humidityLevels) {
            if (hd.isActive) {
                hdCount++;
            }
        }
        hdLenghts = regionMap.GetHumidityLevelsLenght(hdLenghts);
        hdIncRegs = regionMap.GetAllIncludedRegions(hdIncRegs);
        biomsBlend = regionMap.biomsBlend;

        if (useTextures) {
            mainTextures = regionMap.regions.Select(x => x.mainTexture).ToArray();
            slopeTextures = regionMap.regions.Select(x => x.slopeTexture).ToArray();
        }
        scales = GetSizedArray(regionMap.regions.Select(x => x.scale).ToArray(), scales);
        colorStrenghts = GetSizedArray(regionMap.regions.Select(x => x.colorStrenght).ToArray(), colorStrenghts);
        mainColors = GetSizedArray(regionMap.regions.Select(x => x.mainColor).ToArray(), mainColors);
        slopeColors = GetSizedArray(regionMap.regions.Select(x => x.slopeColor).ToArray(), slopeColors);
        heights = GetSizedArray(regionMap.regions.Select(x => x.height).ToArray(), heights);
        slopeThresholds = GetSizedArray(regionMap.regions.Select(x => x.slopeThreshold).ToArray(), slopeThresholds);
        slopeBlendAmounts = GetSizedArray(regionMap.regions.Select(x => x.slopeBlendAmount).ToArray(), slopeBlendAmounts);
        regionBlendAmounts = GetSizedArray(regionMap.regions.Select(x => x.regionBlendAmount).ToArray(), regionBlendAmounts);
    }

    public void SetMaxMinHeights(int heightMapIndex, float maxHeightMultiplier, float minHeightMultiplier)
    {
        maxHeight = NoiseGenerator.GetMaxValue(heightMapIndex) * maxHeightMultiplier;
        minHeight = NoiseGenerator.GetMinValue(heightMapIndex) * minHeightMultiplier;
    }

    Texture2DArray GetTextureArray(Texture2D[] array)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, array.Length, textureFormat, true);
        for (int i = 0; i < array.Length; i++) {
            textureArray.SetPixels(array[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    public void SetShaderValue()
    {
        List<float> hdLenghtsFloat = new List<float>();
        List<float> hdIncRegsFloat = new List<float>();
        foreach (int i in hdLenghts) {
            hdLenghtsFloat.Add(i);
        }
        foreach (int i in hdIncRegs) {
            hdIncRegsFloat.Add(i);
        }
        material.SetFloatArray("hdLenghtsFloat", hdLenghtsFloat.ToArray());
        material.SetFloatArray("hdIncRegsFloat", hdIncRegsFloat.ToArray());
        material.SetFloat("_BiomsBlend", biomsBlend);

        material.SetTexture("_HumidityMap", humidityMap);

        material.SetInt("hdCount", hdCount);

        material.SetInt("mapWidth", mapWidth);
        material.SetInt("mapHeight", mapHeight);
        material.SetInt("chunkWidth", chunkWidth);
        material.SetInt("chunkHeight", chunkHeight);
        material.SetFloat("maxHeight", maxHeight);
        material.SetFloat("minHeight", minHeight);
        material.SetTexture("humidityMap", humidityMap);

        if (useTextures) {
            Texture2DArray mainTexturesArray = GetTextureArray(mainTextures);
            material.SetTexture("mainTexturesArray", mainTexturesArray);
            Texture2DArray slopeTexturesArray = GetTextureArray(slopeTextures);
            material.SetTexture("slopeTexturesArray", slopeTexturesArray);
        }
        material.SetFloatArray("scales", scales);
        material.SetFloatArray("colorStrenghts", colorStrenghts);
        material.SetColorArray("mainColors", mainColors);
        material.SetColorArray("slopeColors", slopeColors);
        material.SetFloatArray("heights", heights);
        material.SetFloatArray("slopeThresholds", slopeThresholds);
        material.SetFloatArray("slopeBlendAmounts", slopeBlendAmounts);
        material.SetFloatArray("regionBlendAmounts", regionBlendAmounts);
    }

}