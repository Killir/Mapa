using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

[CustomEditor(typeof(RegionMap))]
public class RegionMapEditor : Editor
{

    public override void OnInspectorGUI()
    {

        RegionMap regionMap = (RegionMap)target;

        if (regionMap.regions.Count > 0) {
            if (regionMap.regionsMap.Count > 0) {
                foreach (KeyValuePair<int, List<RegionType>> kvp in regionMap.regionsMap) {
                    EditorGUILayout.LabelField(kvp.Key.ToString());
                }
            } else {

                //regionMap.regionsMap.Add()
            }
        } else EditorGUILayout.LabelField("Add some regions!");

        GUILayout.Space(60);

        if (regionMap.regions.Count > 0) {
            foreach (RegionType region in regionMap.regions) {

                EditorGUILayout.BeginVertical("box");

                region.name = EditorGUILayout.TextField("Name", region.name);
                region.height = EditorGUILayout.FloatField("Height", region.height);
                region.humidityId = EditorGUILayout.IntField("Humidity ID", region.humidityId);
                region.color = EditorGUILayout.ColorField("Color", region.color);

                if (GUILayout.Button("Delete", GUILayout.Width(50))) {
                    regionMap.regions.Remove(region);
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        } else {
            EditorGUILayout.LabelField("There is noregions in list!");
        }

        if (GUILayout.Button("Add region")) {
            regionMap.regions.Add(new RegionType());
        }
    }

}
