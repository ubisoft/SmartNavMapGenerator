using UnityEngine;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;
using System.IO;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public TerrainGenerator TerrainGenerator;
    public BuildingGenerator BuildingGenerator;
    public JumppadGenerator JumppadGenerator;
    public GroundGenerator GroundGenerator;
#if UNITY_EDITOR
    [HideInInspector]
    public bool SaveWithGoals = false;
#endif
    [SerializeField]
    private string mapName = "";
    [HideInInspector]
    public string MapName { 
        get 
        {
            if (MapContainer)
                return MapContainer.name;
            else
                return "";
        }
    }
    private GameObject MapContainer;
    private GameObject TerrainContainer;
    private TerrainLoader terrainLoader;

    private void OnEnable()
    {
    }

    private void GenerateGameObjects(string mapName = "")
    {
        DestroyImmediate(MapContainer);
        MapContainer = new GameObject(string.IsNullOrEmpty(mapName) ? RandomWordGenerator.AdjAdjAnimal() : mapName);
        MapContainer.tag = "Map";
        MapContainer.isStatic = true;
        TerrainContainer = new GameObject("Terrain");
        TerrainContainer.tag = "Terrain";
        TerrainContainer.layer = LayerMask.NameToLayer("Terrain");
        TerrainContainer.isStatic = true;
        terrainLoader = TerrainContainer.AddComponent<TerrainLoader>();
        TerrainContainer.transform.parent = MapContainer.transform;
    }

    public void Generate()
    {
        Generate(mapName);
    }

    public void Generate(string mapName = "")
    {
        GenerateGameObjects(mapName);
        NoiseGrid grid = TerrainGenerator.Generate();

        System.Random randomInt = new System.Random(TerrainGenerator.seed);

        BuildingGenerator.Clear();
        BuildingGenerator.Generate(randomInt.Next(), false, TerrainGenerator.Scale, MapContainer);

        Vector3[,] worldBasedHeightMap = HeightMap.WorldBasedHeightMap(grid, TerrainGenerator.HardFloor - grid.Spacing.y, BuildingGenerator.BuildingsGO);
        JumppadGenerator.Generate(worldBasedHeightMap, grid, TerrainGenerator.seed, MapContainer);

        GroundGenerator.Generate(grid, TerrainGenerator.HardFloor, TerrainGenerator.seed, TerrainContainer);
        
        GenerateMesh(grid);
    }

    private void OnDrawGizmos()
    {
        if (TerrainGenerator.Grid != null)
        {
            Gizmos.DrawSphere(new Vector3(0 * TerrainGenerator.Spacing.x * TerrainGenerator.Scale, 0 * TerrainGenerator.Spacing.y * TerrainGenerator.Scale, 0 * TerrainGenerator.Spacing.z * TerrainGenerator.Scale), 0.1f);
            Gizmos.DrawSphere(new Vector3(TerrainGenerator.Grid.GetLength(0) * TerrainGenerator.Spacing.x * TerrainGenerator.Scale, TerrainGenerator.Grid.GetLength(1) * TerrainGenerator.Spacing.y * TerrainGenerator.Scale, TerrainGenerator.Grid.GetLength(2) * TerrainGenerator.Spacing.z * TerrainGenerator.Scale), 0.1f);
        }
    }

    private void GenerateMesh(NoiseGrid grid)
    {
        Mesh terrainMesh = TerrainGenerator.ComputeMesh(grid);
        terrainMesh.name = MapContainer.name;
        terrainLoader.LoadTerrain(terrainMesh);
        TerrainContainer.transform.localScale = Vector3.one * TerrainGenerator.Scale;
    }

    public void SaveMap(string savePath, bool generateSpawnGoals)
    {
        SaveMap(savePath, generateSpawnGoals, MapContainer, terrainLoader, TerrainGenerator, BuildingGenerator, JumppadGenerator, GroundGenerator);
    }

    public static void SaveMap(string savePath, bool generateSpawnGoals, GameObject map, TerrainLoader terrainLoader, TerrainGenerator terrainGenerator, BuildingGenerator buildingGenerator = null, JumppadGenerator jumppadGenerator = null, GroundGenerator groundGenerator = null)
    {
        if (terrainLoader.HasMesh())
        {
            System.IO.Directory.CreateDirectory(savePath);
            JSONNode mapJSON = MapSerializer.Serialize(map);
            System.IO.File.WriteAllText(System.IO.Path.Combine(savePath, MapSerializer.MAP_FILE), mapJSON.ToString(4));

            string modelPath = System.IO.Path.Combine(savePath, MapSerializer.TERRAIN_MESH_FILE);
            System.IO.File.WriteAllBytes(modelPath, terrainLoader.SerializeTerrain());

            Ground[] grounds = map.GetComponentsInChildren<Ground>();
            if (grounds.Length > 0)
            {
                string groundMeshesPath = System.IO.Path.Combine(savePath, "grounds");
                System.IO.Directory.CreateDirectory(groundMeshesPath);
                foreach (Ground ground in grounds)
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(groundMeshesPath, ground.name + ".bin"), ground.Serialize());
            }

            JSONObject config = new JSONObject();
            config.Add(terrainGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(terrainGenerator)));
            if (buildingGenerator)
                config.Add(buildingGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(buildingGenerator)));
            if (jumppadGenerator)
                config.Add(jumppadGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(jumppadGenerator)));
            if (groundGenerator)
                config.Add(groundGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(groundGenerator))); 

            if (generateSpawnGoals)
            {
                SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
                if (spawnGoalGenerator.TotalGoals > 0)
                {
                    spawnGoalGenerator.Clear();
                    spawnGoalGenerator.GenerateNavMesh();
                    List<Vector3[]> spawnGoals = spawnGoalGenerator.GeneralGoals();

                    config.Add(spawnGoalGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(spawnGoalGenerator)));

                    JSONObject json = MapSerializer.SerializeSpawnGoals(spawnGoals);
                    System.IO.File.WriteAllText(System.IO.Path.Combine(savePath, MapSerializer.SPAWN_GOALS_FILE), json.ToString(4));
                }
            }
            System.IO.File.WriteAllText(System.IO.Path.Combine(savePath, MapSerializer.CONFIG_FILE), config.ToString(4));
        }
        else
        {
            Debug.Log("No mesh to save.");
        }
    }

    public bool HasMesh()
    {
        return terrainLoader != null && terrainLoader.HasMesh();
    }

    public string GetMapName()
    {
        return MapContainer.name;
    }

    public void LoadConfiguration(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        if (attr.HasFlag(FileAttributes.Directory))
        {
            path = Path.Combine(path, MapSerializer.CONFIG_FILE);
            LoadConfigurationFromJSON(path);
        }
        else
            LoadConfigurationFromJSON(path);
    }

    public void LoadConfigurationFromJSON(string jsonPath)
    {
        var json = JSON.Parse(System.IO.File.ReadAllText(jsonPath));

        TerrainGenerator terrainGenerator = GameObject.FindObjectOfType<TerrainGenerator>();
        JsonUtility.FromJsonOverwrite(json[terrainGenerator.GetType().Name].ToString(), terrainGenerator);

        BuildingGenerator buildingGenerator = GameObject.FindObjectOfType<BuildingGenerator>();
        JsonUtility.FromJsonOverwrite(json[buildingGenerator.GetType().Name].ToString(), buildingGenerator);

        SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
        if (json[spawnGoalGenerator.GetType().Name] != null)
            JsonUtility.FromJsonOverwrite(json[spawnGoalGenerator.GetType().Name].ToString(), spawnGoalGenerator);

        JumppadGenerator jumppadGenerator = GameObject.FindObjectOfType<JumppadGenerator>();
        if (json[jumppadGenerator.GetType().Name] != null)
            JsonUtility.FromJsonOverwrite(json[jumppadGenerator.GetType().Name].ToString(), jumppadGenerator);

        GroundGenerator groundGenerator = GameObject.FindObjectOfType<GroundGenerator>();
        if (json[groundGenerator.GetType().Name] != null)
            JsonUtility.FromJsonOverwrite(json[groundGenerator.GetType().Name].ToString(), groundGenerator);
    }
}
