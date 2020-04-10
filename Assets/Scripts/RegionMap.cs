using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RegionMap : MonoBehaviour
{

    public Dictionary<int,List<RegionType>> regionsMap = new Dictionary<int, List<RegionType>>();
    public List<RegionType> regions = new List<RegionType>();
}

[System.Serializable]
public class RegionType
{
    public string name;
    public float height;
    public int humidityId;
    public Color color;
}
