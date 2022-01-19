using UnityEditor;
using UnityEngine;
using LaForge.MapGenerator.SimpleJSON;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        EditorGUI.BeginDisabledGroup(!terrainGenerator.isActiveAndEnabled);
        if (GUILayout.Button("Load config"))
        {
            string path = EditorUtility.OpenFilePanel("Select a generator configuration", "", "json");
            var json = JSON.Parse(System.IO.File.ReadAllText(path));
            JsonUtility.FromJsonOverwrite(json[terrainGenerator.GetType().Name].ToString(), terrainGenerator);
        }
    }
}
