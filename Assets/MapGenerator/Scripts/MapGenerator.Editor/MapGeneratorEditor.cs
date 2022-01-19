using UnityEditor;
using UnityEngine;
using LaForge.MapGenerator;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;
        MapGenerator mapRandomizer = (MapGenerator)target;

        EditorGUI.BeginDisabledGroup(!mapRandomizer.isActiveAndEnabled);

        if(GUILayout.Button("Ranomize Seed"))
        {
            var rng = new System.Random();
            mapRandomizer.TerrainGenerator.seed = rng.Next(1000000);
        }

        if(GUILayout.Button("Generate"))
        {
            Debug.Log("generating using seed: " + mapRandomizer.TerrainGenerator.seed);
            mapRandomizer.Generate();
        }

        if (GUILayout.Button("Load Configuration"))
        {
            string path = EditorUtility.OpenFolderPanel("Select a map folder", "", "");
            if (!string.IsNullOrEmpty(path))
                mapRandomizer.LoadConfiguration(path);
        }

        EditorGUI.BeginDisabledGroup(!mapRandomizer.HasMesh());
        bool shouldSaveMap = GUILayout.Button("Save Map");
        mapRandomizer.SaveWithGoals = GUILayout.Toggle(mapRandomizer.SaveWithGoals, "Save with goals");

        if (mapRandomizer.SaveWithGoals)
        {
            FindObjectOfType<SpawnGoalGenerator>().TotalGoals = EditorGUILayout.IntField("Spawn-Goals", FindObjectOfType<SpawnGoalGenerator>().TotalGoals);
        }

        if (shouldSaveMap)
        {
            string path = EditorUtility.SaveFolderPanel("Select a folder to save the map", "", "");
            mapRandomizer.SaveMap(System.IO.Path.Combine(path, mapRandomizer.MapName), mapRandomizer.SaveWithGoals);
        }

        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
    }
}
