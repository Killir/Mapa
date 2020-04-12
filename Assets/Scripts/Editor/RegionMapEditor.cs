using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RegionMap))]
public class RegionMapEditor : Editor
{

    RegionMap serializableObject;

    public override void OnInspectorGUI()
    {
        serializableObject = (RegionMap)target;

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

                if (hd.includedRegions.Count > 0) {     // Included regions

                    EditorGUILayout.LabelField("Regions: ");
                    for (int i = 0; i < hd.includedRegions.Count; i++) {
                        GUILayout.BeginHorizontal();

                        hd.includedRegions[i].index = EditorGUILayout.Popup(hd.includedRegions[i].index, regionsNames.ToArray(), GUILayout.Width(Screen.width - 100));
                        hd.includedRegions[i].SetRegion(serializableObject.regions[hd.includedRegions[i].index]);

                        if (GUILayout.Button("Delete", GUILayout.Width(50))) {
                            hd.includedRegions.RemoveAt(i);
                        }

                        GUILayout.EndHorizontal();
                    }
                } else EditorGUILayout.LabelField("Region list is empty!");

                GUILayout.Space(5);
                if (GUILayout.Button("Add", GUILayout.Width(50))) {
                    hd.includedRegions.Add(new IncludedRegion());
                }

                GUILayout.Space(15);
                if (GUILayout.Button("Delete humidity level")) {
                    serializableObject.humidityLevels.Remove(hd);
                    serializableObject.UpdateIndices();
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        } else {
            EditorGUILayout.LabelField("There is no humidity levels in list!");
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add humidity level")) {
            serializableObject.humidityLevels.Add(new HumidityData(serializableObject.humidityLevels.Count));
        }

        if (GUILayout.Button("Log")) {
            serializableObject.LogHumidityLevels();
        }

        GUILayout.Space(50);

        if (serializableObject.regions.Count > 0) { //Regions

            int regionCount = serializableObject.regions.Count;
            for (int i = 0; i < regionCount; i++) {
                RegionData region = serializableObject.regions[i];

                EditorGUILayout.BeginVertical("box");

                region.name = EditorGUILayout.TextField("Name", region.name);
                region.height = EditorGUILayout.Slider("Height", region.height, 0f, 1f);
                region.color = EditorGUILayout.ColorField("Color", region.color);

                GUILayout.Space(15);

                EditorGUILayout.BeginHorizontal();

                if (i > 0) {
                    if (GUILayout.Button("Up", GUILayout.Width(50))) {
                        serializableObject.SwapRegionUp(i);
                    }
                } else GUILayout.Space(52);

                if (i < regionCount - 1) {
                    if (GUILayout.Button("Down", GUILayout.Width(50))) {
                        serializableObject.SwapRegionDown(i);
                    }
                } else GUILayout.Space(52);

                GUILayout.Space(Screen.width - 200);

                
                if (GUILayout.Button("Delete", GUILayout.Width(50))) {
                    serializableObject.DeleteRegion(region);
                    break;
                }

                EditorGUILayout.EndHorizontal();

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
