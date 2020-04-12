using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RegionMap))]
public class RegionMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RegionMap serializableObject = (RegionMap)target;

        List<string> regionsNames = new List<string>();
        if (serializableObject.regions.Count > 0) {
            foreach (RegionData rd in serializableObject.regions) {
                regionsNames.Add(rd.name);
            }
        }

        if (serializableObject.humidityLevels.Count > 0) { // humidity levels
            for (int j = 0; j < serializableObject.humidityLevels.Count; j++)  {

                HumidityData hd = serializableObject.humidityLevels[j];

                EditorGUILayout.BeginVertical("box");

                hd.name = EditorGUILayout.TextField("Name", hd.name);
                hd.level = EditorGUILayout.IntField("Level", hd.level);

                if (hd.includedRegions.Count > 0) {     // Included regions

                    EditorGUILayout.LabelField("Regions: ");
                    for (int i = 0; i < hd.includedRegions.Count; i++) {
                        GUILayout.BeginHorizontal();

                        hd.includedRegions[i].index = EditorGUILayout.Popup(hd.includedRegions[i].index, regionsNames.ToArray(), GUILayout.Width(Screen.width - 100));

                        if (GUILayout.Button("Delete", GUILayout.Width(50))) {
                            hd.includedRegions.RemoveAt(i);
                        }

                        GUILayout.EndHorizontal();
                    }
                } else EditorGUILayout.LabelField("Region list is empty!");

                GUILayout.Space(5);
                if (GUILayout.Button("Add", GUILayout.Width(50))) {
                    hd.includedRegions.Add(new HumidityData.IncludedRegion());
                }

                GUILayout.Space(15);
                if (GUILayout.Button("Delete humidity level")) {
                    serializableObject.humidityLevels.Remove(hd);
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        } else {
            EditorGUILayout.LabelField("There is no humidity levels in list!");
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add humidity level")) {
            serializableObject.humidityLevels.Add(new HumidityData());
        }

        GUILayout.Space(50);

        if (serializableObject.regions.Count > 0) { //Regions

            foreach (RegionData region in serializableObject.regions) {

                EditorGUILayout.BeginVertical("box");

                region.name = EditorGUILayout.TextField("Name", region.name);
                region.height = EditorGUILayout.Slider("Height", region.height, 0f, 1f);
                region.humidityId = EditorGUILayout.IntField("Humidity ID", region.humidityId);
                region.color = EditorGUILayout.ColorField("Color", region.color);

                if (GUILayout.Button("Delete", GUILayout.Width(50))) {
                    serializableObject.regions.Remove(region);
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        } else {
            EditorGUILayout.LabelField("There is no regions in list!");
        }

        if (GUILayout.Button("Add region")) {
            serializableObject.regions.Add(new RegionData());
        }


        if (GUI.changed) {
            EditorUtility.SetDirty(serializableObject);
            EditorSceneManager.MarkSceneDirty(serializableObject.gameObject.scene);
        }
    }

}
