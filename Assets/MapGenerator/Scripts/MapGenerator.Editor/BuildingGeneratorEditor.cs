using UnityEngine;
using UnityEditor;
using LaForge.MapGenerator.SimpleJSON;

[InitializeOnLoad]
[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    SerializedProperty platformGenerationStep;
    private void OnEnable()
    {
        platformGenerationStep = serializedObject.FindProperty("PlatformGenerationStep");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Buildings Settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Number of Buildings");
        using (new EditorGUI.IndentLevelScope())
        {
            platformGenerationStep.FindPropertyRelative("MinNbPlatforms").intValue = EditorGUILayout.IntField("Minimum", platformGenerationStep.FindPropertyRelative("MinNbPlatforms").intValue);
            platformGenerationStep.FindPropertyRelative("MaxNbPlatforms").intValue = EditorGUILayout.IntField("Maximum", platformGenerationStep.FindPropertyRelative("MaxNbPlatforms").intValue);
        }

        EditorGUILayout.LabelField("Platforms Radius");
        using (new EditorGUI.IndentLevelScope())
        {
            platformGenerationStep.FindPropertyRelative("InnerRadius").floatValue = EditorGUILayout.FloatField("Inner", platformGenerationStep.FindPropertyRelative("InnerRadius").floatValue);
            platformGenerationStep.FindPropertyRelative("OuterRadius").floatValue = EditorGUILayout.FloatField("Outer", platformGenerationStep.FindPropertyRelative("OuterRadius").floatValue);
        }

        EditorGUILayout.LabelField("Platforms Height");
        using (new EditorGUI.IndentLevelScope())
        {
            platformGenerationStep.FindPropertyRelative("MinHeight").floatValue = EditorGUILayout.FloatField("Minimum", platformGenerationStep.FindPropertyRelative("MinHeight").floatValue);
            platformGenerationStep.FindPropertyRelative("MaxHeight").floatValue = EditorGUILayout.FloatField("Maximum", platformGenerationStep.FindPropertyRelative("MaxHeight").floatValue);
        }

        platformGenerationStep.FindPropertyRelative("NonOverlapping").boolValue = EditorGUILayout.Toggle("Non Overlapping Platforms", platformGenerationStep.FindPropertyRelative("NonOverlapping").boolValue);
        platformGenerationStep.FindPropertyRelative("PlatformsWarping").floatValue = EditorGUILayout.Slider("Platforms Warping", platformGenerationStep.FindPropertyRelative("PlatformsWarping").floatValue, 0, 1);
        platformGenerationStep.FindPropertyRelative("Tries").intValue = EditorGUILayout.IntField("Tries", platformGenerationStep.FindPropertyRelative("Tries").intValue);

        serializedObject.ApplyModifiedProperties();


        BuildingGenerator procGen = (BuildingGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            procGen.Generate();
        }
        
        else if (GUILayout.Button("Clear"))
        {
            procGen.Clear();
        }

        //else if (GUI.changed)
        //{
        //    procGen.Generate();
        //}

        if (GUILayout.Button("Load config"))
        {
            string path = EditorUtility.OpenFilePanel("Select a generator configuration", "", "json");
            var json = JSON.Parse(System.IO.File.ReadAllText(path));
            JsonUtility.FromJsonOverwrite(json[procGen.GetType().Name].ToString(), procGen);
        }
    }
}
