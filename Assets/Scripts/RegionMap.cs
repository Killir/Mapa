using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RegionMap : MonoBehaviour
{
    public List<HumidityData> humidityLevels = new List<HumidityData>();
    public List<RegionData> regions = new List<RegionData>();

}

[System.Serializable]
public class HumidityData
{
    public string name;
    public int level;
    public List<IncludedRegion> includedRegions = new List<IncludedRegion>();

    [System.Serializable]
    public class IncludedRegion
    {
        public RegionData region;
        public int index;
    }
}

[System.Serializable]
public class RegionData
{
    public string name;
    public float height;
    public int humidityId;
    public Color color;
}
