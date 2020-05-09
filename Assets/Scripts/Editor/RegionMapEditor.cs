using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RegionMap))]
public class RegionMapEditor : Editor
{

    RegionMap serializableObject;

    const int maxHDCount = 8;
    const int maxIncRegCount = 16;
    const int maxRegionCount = 128;

    bool autoUpdateShader;

    public override void OnInspectorGUI()
    {
        serializableObject = (RegionMap)target;        

        List<string> regionsNames = new List<string>();
        if (serializableObject.regions.Count > 0) {
            foreach (RegionData rd in serializableObject.regions) {
                regionsNames.Add(rd.name);
            }
        }

        serializableObject.biomsBlend = EditorGUILayout.Slider("Bioms blend amount", serializableObject.biomsBlend, 0f, 0.5f);

        if (serializableObject.humidityLevels.Count > 0) { // humidity levels
            
            for (int j = 0; j < serializableObject.humidityLevels.Count; j++)  {

                HumidityData hd = serializableObject.humidityLevels[j];

                EditorGUILayout.BeginVertical("box");

                //GUILayout.BeginHorizontal();
                hd.isActive = EditorGUILayout.Toggle(hd.isActive);
                serializableObject.UpdateIndices();
                hd.name = EditorGUILayout.TextField("Name", hd.name);
                //GUILayout.EndHorizontal();

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
                if (serializableObject.regions.Count > 0 && hd.includedRegions.Count < maxIncRegCount) {
                    if (GUILayout.Button("Add", GUILayout.Width(50))) {
                        hd.includedRegions.Add(new IncludedRegion());
                        serializableObject.UpdateIndices();
                    }
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
        if (serializableObject.humidityLevels.Count < maxHDCount) {
            if (GUILayout.Button("Add humidity level")) {
                serializableObject.humidityLevels.Add(new HumidityData(serializableObject.humidityLevels.Count));
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Sort included regions")) {
            serializableObject.SortRegionDataList();
        }
        if (GUILayout.Button("Log")) {
            serializableObject.LogHumidityLevels();
        }

        if (GUILayout.Button("Update")) {
            serializableObject.UpdateIndices();
        }

        GUILayout.Space(50);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto update shader");        
        autoUpdateShader = EditorGUILayout.Toggle(autoUpdateShader);
        GUILayout.EndHorizontal();

        serializableObject.textureSize = EditorGUILayout.IntField("Texture size", serializableObject.textureSize);

        if (serializableObject.regions.Count > 0) { //Regions

            int regionCount = serializableObject.regions.Count;
            for (int i = 0; i < regionCount; i++) {
                RegionData region = serializableObject.regions[i];

                EditorGUILayout.BeginVertical("box");

                region.name = EditorGUILayout.TextField("Name", region.name);
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Main texture");
                region.mainTexture = (Texture2D)EditorGUILayout.ObjectField(region.mainTexture, typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Slope texture");
                region.slopeTexture = (Texture2D)EditorGUILayout.ObjectField(region.slopeTexture, typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                region.scale = EditorGUILayout.FloatField("Scale", region.scale);
                GUILayout.Space(10);

                region.colorStrenght = EditorGUILayout.Slider("Color strenght", region.colorStrenght, 0f, 1f);
                region.height = EditorGUILayout.Slider("Height", region.height, 0f, 1f);
                region.mainColor = EditorGUILayout.ColorField("Main color", region.mainColor);
                region.slopeColor = EditorGUILayout.ColorField("Slope color", region.slopeColor);
                region.slopeThreshold = EditorGUILayout.Slider("Slope threshold", region.slopeThreshold, 0f, 1f);
                region.slopeBlendAmount = EditorGUILayout.Slider("Slope blend amount", region.slopeBlendAmount, 0f, 1f);
                region.regionBlendAmount = EditorGUILayout.Slider("Region blend amount", region.regionBlendAmount, 0f, 0.5f);

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

        if (serializableObject.regions.Count < maxRegionCount) {
            if (GUILayout.Button("Add region")) {
                serializableObject.regions.Add(new RegionData());
            }
        }

        if (GUI.changed) {
            if (autoUpdateShader) {
                serializableObject.UpdateAndApplyShaderData();
            }
            EditorUtility.SetDirty(serializableObject);
            EditorSceneManager.MarkSceneDirty(serializableObject.gameObject.scene);
        }
    }

}
