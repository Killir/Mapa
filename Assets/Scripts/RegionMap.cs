using System.Collections.Generic;
using UnityEngine;


public class RegionMap : MonoBehaviour
{
    public List<HumidityData> humidityLevels = new List<HumidityData>();
    public List<RegionData> regions = new List<RegionData>();

    public RegionData Evaluate(float height, float humidity)
    {
        RegionData region = new RegionData();
        List<RegionData> currentHumidityLevelRegions = new List<RegionData>();
        int humiditylevelIndex = Mathf.RoundToInt(Mathf.Lerp(0f, humidityLevels.Count - 1, humidity));

        foreach (HumidityData hd in humidityLevels) {
            if (hd.GetIndex() == humiditylevelIndex) {
                foreach (IncludedRegion incReg in hd.includedRegions) {
                    currentHumidityLevelRegions.Add(incReg.GetRegion());
                }
                break;
            }
        }

        currentHumidityLevelRegions = SortRegionDataList(currentHumidityLevelRegions);
        currentHumidityLevelRegions[0].height = 0f;

        foreach (RegionData rd in currentHumidityLevelRegions) {
            if (height >= rd.height) {
                region = rd;
            } else 
                break;
        }

        return region;
    }

    public void UpdateIndices()
    {
        int i = 0;
        foreach(HumidityData hd in humidityLevels) {
            hd.SetIndex(i);
            i++;
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

        UpdateIndices();

    }

    public void SwapRegionUp(int index)
    {

        RegionData temp = regions[index];
        regions[index] = regions[index - 1];
        regions[index - 1] = temp;

        UpdateIndices();

    }

    List<RegionData> SortRegionDataList(List<RegionData> list)
    {
        for (int i = 0; i < list.Count; i++) {
            for (int j = 0; j < list.Count - (i + 1); j++) {
                if (list[j].height > list[j+1].height) {
                    RegionData temp = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = temp;
                }
            }
        }

        return list;
    }

    public void LogHumidityLevels()
    {
        foreach (HumidityData hd in humidityLevels) {
            string s = hd.name;
            s += "\n" + "Index: " + hd.GetIndex().ToString();
            s += "\n" + "Included regions count: " + hd.includedRegions.Count.ToString();
            Debug.Log(s);
        }
    }

}

[System.Serializable]
public class HumidityData
{
    public string name;
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
    public string name;
    public float height;
    public Color color;
}
