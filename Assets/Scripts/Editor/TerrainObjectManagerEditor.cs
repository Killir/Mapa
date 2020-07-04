using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainObjectManager))]
public class TerrainObjectManagerEditor : Editor
{
    TerrainObjectManager serializableObject;

    void DisplayObject(TerrainObjectData tod, int index, out bool isDeleted)
    {
        GUILayout.BeginHorizontal("box");

        GUILayout.BeginVertical();
        tod.obj = EditorGUILayout.ObjectField("Object", tod.obj, typeof(GameObject), false) as GameObject;
        tod.spawnRadius = EditorGUILayout.FloatField("Spawn radius",tod.spawnRadius);
        tod.spawnIterationCount = EditorGUILayout.IntField("Spawn iteration", tod.spawnIterationCount);
        tod.minSpawnHeight = EditorGUILayout.FloatField("Min height", tod.minSpawnHeight);
        tod.maxSpawnHeight = EditorGUILayout.FloatField("Max height", tod.maxSpawnHeight);
        tod.spawnHeightOffset = EditorGUILayout.FloatField("Height offset", tod.spawnHeightOffset);
        tod.maxSpawnSlopeValue = EditorGUILayout.Slider("Max slope", tod.maxSpawnSlopeValue, 0f, 0.1f);
        tod.scaleFactor = EditorGUILayout.Slider("Scale factor", tod.scaleFactor, 0f, 0.99f);
        GUILayout.EndVertical();

        if (GUILayout.Button("Delete object", GUILayout.Height(40))) {
            serializableObject.objects.RemoveAt(index);
            isDeleted = true;
        } else isDeleted = false;
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        serializableObject = (TerrainObjectManager)target;

        if (serializableObject.objects.Count > 0) {
            int i = 0;
            foreach (TerrainObjectData tod in serializableObject.objects) {
                bool isDeleted = false;
                DisplayObject(tod, i, out isDeleted);
                GUILayout.Space(10);
                if (isDeleted)
                    break;
                i++;
            }
        } else {
            EditorGUILayout.LabelField("Objects list is empty");
        }

        if (GUILayout.Button("Add object")) {
            serializableObject.objects.Add(new TerrainObjectData());
        }
    }

}
