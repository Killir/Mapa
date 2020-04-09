using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{

    bool generationIsAllowed = false;

    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        if (mapGenerator.chunkSize * (mapGenerator.levelOfDetail * 2) * mapGenerator.chunkSize * (mapGenerator.levelOfDetail * 2) >= 65000) {
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

    }
}
