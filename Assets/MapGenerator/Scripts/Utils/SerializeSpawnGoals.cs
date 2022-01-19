#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using LaForge.MapGenerator;

[ExecuteInEditMode]
[RequireComponent(typeof(SpawnGoalGenerator))]
public class SerializeSpawnGoals : MonoBehaviour
{
    [HideInInspector] [SerializeField] Vector3 serializableSpawn;
    [HideInInspector] [SerializeField] Vector3 serializableGoal;
    [HideInInspector] public Vector3[] spawnGoal;
    [HideInInspector] [SerializeField] bool serialized = false;

    // Use this for initialization

    void Awake()
    {
        if (serialized)
        {
            spawnGoal = new Vector3[] { serializableSpawn, serializableGoal };
        }
    }

    void Start()
    {
        if (serialized) return;

        Serialize();
    }

    public void Serialize()
    {
        List<Vector3[]> spawnGoalsVec3 = GetComponent<SpawnGoalGenerator>().GeneralGoals();
        serializableSpawn = spawnGoalsVec3[0][0];
        serializableGoal = spawnGoalsVec3[0][1];
        serialized = true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SerializeSpawnGoals))]
class SerializeSpawnGoalsEditor : Editor
{
    SerializeSpawnGoals obj;

    void OnSceneGUI()
    {
        obj = (SerializeSpawnGoals)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Serialize"))
        {
            if (obj)
            {
                obj.Serialize();
            }
        }
    }
}
#endif

[System.Serializable]
public class SerializableVector3
{
    public float x = 0;
    public float y = 0;
    public float z = 0;

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

