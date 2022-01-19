using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapLoader))]
public class MapLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;
        MapLoader mapLoader = (MapLoader)target;

        EditorGUI.BeginDisabledGroup(!mapLoader.isActiveAndEnabled);
        if (GUILayout.Button("Load Map"))
        {
            string path = EditorUtility.OpenFolderPanel("Select a map folder", "", "");
            if (!string.IsNullOrEmpty(path))
                mapLoader.LoadMap(path);
        }

        if (GUILayout.Button("Load Spawn-Goal and Map"))
        {
            string path = EditorUtility.OpenFolderPanel("Select a map folder", "", "");
            if (!string.IsNullOrEmpty(path))
                mapLoader.LoadSpawnGoalsAndMap(path);
        }
        EditorGUI.EndDisabledGroup();
    }
}
