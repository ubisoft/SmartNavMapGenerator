using UnityEngine;
using UnityEditor;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

[CustomEditor(typeof(SpawnGoalGenerator))]
public class SpawnGoalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawnGoalGenerator spawnGoalGenerator = (SpawnGoalGenerator)target;

        if (GUILayout.Button("Generate Points"))
        {
            spawnGoalGenerator.GeneralGoals();
        }

        if (GUILayout.Button("Generate NavMesh"))
        {
            spawnGoalGenerator.GenerateNavMesh();
        }

        else if (GUILayout.Button("Clear"))
        {
            spawnGoalGenerator.Clear();
        }

        if (GUILayout.Button("Load config"))
        {
            string path = EditorUtility.OpenFilePanel("Select a generator configuration", "", "json");
            var json = JSON.Parse(System.IO.File.ReadAllText(path));
            JsonUtility.FromJsonOverwrite(json[spawnGoalGenerator.GetType().Name].ToString(), spawnGoalGenerator);
        }
    }
}
