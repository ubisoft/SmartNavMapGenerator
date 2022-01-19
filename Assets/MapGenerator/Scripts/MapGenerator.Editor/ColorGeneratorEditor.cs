using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorGenerator))]
public class ColorGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColorGenerator colorGenerator = (ColorGenerator)target;

        EditorGUI.BeginDisabledGroup(!colorGenerator.isActiveAndEnabled);
        if (GUILayout.Button("Export gradient"))
        {
            string path = EditorUtility.SaveFilePanel("Save map color gradient as PNG", "./Assets/Map Loading/Resources/Map Gradients/", "gradient", "png");
            if (!System.String.IsNullOrEmpty(path))
                colorGenerator.ExportTexture(path);
        }
        EditorGUI.EndDisabledGroup();
    }
}
