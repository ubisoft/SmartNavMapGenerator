using UnityEngine;
using System.Collections.Generic;
using LaForge.MapGenerator;

public class MapLoader : MonoBehaviour
{
    [Header("Options")]
    [SerializeField]
    public bool debugDisplay = false;

    private List<GameObject> GameObjects = new List<GameObject>();

    public void LoadMap(string mapJSONPath)
    {
        GameObject go = MapSerializer.LoadMap(mapJSONPath);
        go.transform.position = this.gameObject.transform.position;
    }

    public void LoadSpawnGoalsAndMap(string jsonPath)
    {
        MapSerializer.SpawnGoalAndMap? result = MapSerializer.LoadSpawnGoalsAndMap(jsonPath);
        if (result.HasValue)
        {
            result.Value.Map.transform.position = this.gameObject.transform.position;

            Debug.Log(result.Value.SpawnsGoals);

            if (debugDisplay)
            {
                if (GameObjects.Count > 0)
                {
                    foreach(GameObject obj in GameObjects)
                    {
                        DestroyImmediate(obj);
                    }
                }

                for (int i = 0; i < result.Value.SpawnsGoals.Count; i++)
                {
                    GameObject cubeSpawnGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cubeSpawnGo.name = "Spawn_" + i;
                    Material cubeSpawnMaterial = new Material(Shader.Find("Diffuse"));
                    cubeSpawnMaterial.color = Color.green;
                    cubeSpawnGo.GetComponent<Renderer>().sharedMaterial = cubeSpawnMaterial;
                    cubeSpawnGo.transform.localScale = 0.25f * Vector3.one;
                    cubeSpawnGo.transform.parent = result.Value.Map.transform;

                    GameObject sphereGoalGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphereGoalGo.name = "Goal_" + i;
                    Material sphereGoalMaterial = new Material(Shader.Find("Diffuse"));
                    sphereGoalMaterial.color = Color.red;
                    sphereGoalGo.GetComponent<Renderer>().sharedMaterial = sphereGoalMaterial;
                    sphereGoalGo.transform.localScale = 0.25f * Vector3.one;
                    sphereGoalGo.transform.parent = result.Value.Map.transform;

                    Vector3 spawn = result.Value.SpawnsGoals[i][0];
                    Vector3 goal = result.Value.SpawnsGoals[i][1];

                    cubeSpawnGo.transform.position = spawn;
                    sphereGoalGo.transform.position = goal;
                    Debug.DrawLine(spawn, goal, Color.blue, 3f, false);

                    GameObjects.Add(cubeSpawnGo);
                    GameObjects.Add(sphereGoalGo);
                }
            }
        }
    }
}
