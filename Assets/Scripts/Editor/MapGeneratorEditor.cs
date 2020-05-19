using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{

    bool generationIsAllowed = false;

    void SaveTerrainAsAsset()
    {
        foreach (KeyValuePair<Vector2, TerrainData> kvp in MapGenerator.terrainDataDictionary) {
            GameObject obj = kvp.Value.GetGameObject();
            string path = "Assets/Saved Terrains/" + obj.name + ".prefab";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            GameObject prefRef = Instantiate(obj);
            Mesh prefMesh = obj.GetComponent<MeshFilter>().sharedMesh;
            GameObject pref = PrefabUtility.SaveAsPrefabAssetAndConnect(prefRef, path, InteractionMode.UserAction);
            AssetDatabase.AddObjectToAsset(prefMesh, path);
            pref.GetComponent<MeshFilter>().sharedMesh = prefMesh;

            AssetDatabase.SaveAssets();
            DestroyImmediate(prefRef);
        }
    }

    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        int LOD = mapGenerator.levelOfDetail == 0 ? 1 : mapGenerator.levelOfDetail * 2;
        if (mapGenerator.chunkSize * (LOD) * mapGenerator.chunkSize * (LOD) >= 65000) {
            generationIsAllowed = false;
            EditorGUILayout.HelpBox("The number of verticles should not exceed 65000", MessageType.Error);
        } else {
            generationIsAllowed = true;
        }

        if (DrawDefaultInspector()) {
            if (mapGenerator.autoUpdate && generationIsAllowed) {
                mapGenerator.GenerateMap();
            }            
        }

        if (GUILayout.Button("Generate")) {
            mapGenerator.GenerateMap();
        }

        if (GUILayout.Button("New seed")) {
            mapGenerator.SetNewSeed();
            mapGenerator.GenerateMap();
        }

        if (GUILayout.Button("Clear Terrain")) {
            mapGenerator.ClearTerrainDictionary();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Save terrain as prefab")) {
            SaveTerrainAsAsset();
        }

    }
}
