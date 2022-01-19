using UnityEditor;
using UnityEngine;
using LaForge.MapGenerator;

[CustomEditor(typeof(TerrainLoader))]
public class TerrainLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;
        TerrainLoader terrainLoader = (TerrainLoader)target;
        EditorGUI.BeginDisabledGroup(!terrainLoader.isActiveAndEnabled);
        if (GUILayout.Button("Load Gradient"))
        {
            string path = EditorUtility.OpenFilePanel("Select a gradient", "", "png");
            terrainLoader.LoadGradient(path);
        }
        EditorGUI.EndDisabledGroup();
    }
}
