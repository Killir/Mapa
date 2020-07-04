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

    int regionCount = 0;
    bool autoUpdateShader;
    bool useTextures = false;

    List<string> InitializeRegionNamesList()
    {
        List<string> regionNames = new List<string>();
        if (serializableObject.regions.Count > 0) {
            foreach (RegionData rd in serializableObject.regions) {
                regionNames.Add(rd.name);
            }
        }
        return regionNames;
    }

    public override void OnInspectorGUI()
    {
        serializableObject = (RegionMap)target;
        regionCount = serializableObject.regions.Count;
        List<string> regionNames = InitializeRegionNamesList();

        DisplaySettings();

        // humidity levels /////////////////////////////////////

        if (serializableObject.humidityLevels.Count > 0) {             
            for (int j = 0; j < serializableObject.humidityLevels.Count; j++)  {
                DisplayHumidityLevel(serializableObject.humidityLevels[j], regionNames);               
            }
        } else {
            EditorGUILayout.LabelField("There is no humidity levels in list!");
        }

        if (serializableObject.humidityLevels.Count < maxHDCount) {
            GUILayout.Space(15);
            if (GUILayout.Button("Add humidity level", GUILayout.Height(30))) {
                serializableObject.humidityLevels.Add(new HumidityData(serializableObject.humidityLevels.Count));
            }
        }

        GUILayout.Space(50);
        // Regions /////////////////////////////////////////////

        if (regionCount > 0) {
            for (int i = 0; i < regionCount; i++) {
                DisplayRegion(serializableObject.regions[i], i);                
            }
        } else {
            EditorGUILayout.LabelField("There is no regions in list!");
        }

        if (regionCount < maxRegionCount) {
            GUILayout.Space(15);
            if (GUILayout.Button("Add region", GUILayout.Height(30))) {
                serializableObject.regions.Add(new RegionData());
            }
        }

        GUILayout.Space(50);

        if (GUI.changed) {
            if (autoUpdateShader) {
                serializableObject.UpdateAndApplyShaderData();
            }
            EditorUtility.SetDirty(serializableObject);
            EditorSceneManager.MarkSceneDirty(serializableObject.gameObject.scene);
        }
    }

    bool DisplayBooleanField(string name, bool value)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name);
        value = EditorGUILayout.Toggle(value);
        GUILayout.EndHorizontal();

        return value;
    }

    void DisplaySettings()
    {
        serializableObject.biomsBlend = EditorGUILayout.Slider("Bioms blend amount", serializableObject.biomsBlend, 0f, 0.5f);
        serializableObject.useTextures = DisplayBooleanField("Use textures", serializableObject.useTextures);
        serializableObject.textureSize = EditorGUILayout.IntField("Texture size", serializableObject.textureSize);
        autoUpdateShader = DisplayBooleanField("Auto update shader", autoUpdateShader);

        GUILayout.Space(10);
        if (GUILayout.Button("Sort included regions")) {
            serializableObject.SortRegionDataList();
        }
        if (GUILayout.Button("Log")) {
            serializableObject.LogHumidityLevels();
        }
        if (GUILayout.Button("Update indices")) {
            serializableObject.UpdateIndices();
        }
        if (GUILayout.Button("Update shader data")) {
            serializableObject.UpdateAndApplyShaderData();
        }


        if (serializableObject.useTextures != useTextures) {
            useTextures = serializableObject.useTextures;
            serializableObject.SwitchColorStrenghts(useTextures);
        }

    }

    void DisplayHumidityLevel(HumidityData hd, List<string> regionNames)
    {
        EditorGUILayout.BeginVertical("box");

        hd.isActive = EditorGUILayout.Toggle(hd.isActive);
        serializableObject.UpdateIndices();        
        hd.name = EditorGUILayout.TextField("Name", hd.name);

        if (hd.includedRegions.Count > 0) {     // Included regions

            EditorGUILayout.LabelField("Regions: ");
            for (int i = 0; i < hd.includedRegions.Count; i++) {
                GUILayout.BeginHorizontal();

                hd.includedRegions[i].index = EditorGUILayout.Popup(hd.includedRegions[i].index, regionNames.ToArray(), GUILayout.Width(Screen.width - 100));
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
            return;
        }

        EditorGUILayout.EndVertical();
    }

    void DisplayRegion(RegionData region, int index)
    {
        EditorGUILayout.BeginVertical("box");

        region.name = EditorGUILayout.TextField("Name", region.name);
        region.height = EditorGUILayout.Slider("Height", region.height, 0f, 1f);

        if (useTextures) {
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
        }

        region.colorStrenght = EditorGUILayout.Slider("Color strenght", region.colorStrenght, 0f, 1f);
        region.mainColor = EditorGUILayout.ColorField("Main color", region.mainColor);
        region.slopeColor = EditorGUILayout.ColorField("Slope color", region.slopeColor);
        region.slopeThreshold = EditorGUILayout.Slider("Slope threshold", region.slopeThreshold, 0f, 1f);
        region.slopeBlendAmount = EditorGUILayout.Slider("Slope blend amount", region.slopeBlendAmount, 0f, 1f);
        region.regionBlendAmount = EditorGUILayout.Slider("Region blend amount", region.regionBlendAmount, 0f, 0.5f);

        GUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();

        if (index > 0) {
            if (GUILayout.Button("Up", GUILayout.Width(50))) {
                serializableObject.SwapRegionUp(index);
            }
        } else GUILayout.Space(52);

        if (index < regionCount - 1) {
            if (GUILayout.Button("Down", GUILayout.Width(50))) {
                serializableObject.SwapRegionDown(index);
            }
        } else GUILayout.Space(52);

        GUILayout.Space(Screen.width - 200);


        if (GUILayout.Button("Delete", GUILayout.Width(50))) {
            serializableObject.DeleteRegion(region);
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

}
