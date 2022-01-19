using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LaForge.MapGenerator
{
    public static class MapSerializer
    {
        public static string MAP_FILE { get { return "map.json"; } }
        public static string CONFIG_FILE { get { return "config.json"; } }
        public static string SPAWN_GOALS_FILE { get { return "spawn_goals.json"; } }
        public static string TERRAIN_MESH_FILE { get { return "terrain.bin"; } }
        public static SimpleJSON.JSONNode Serialize(GameObject map)
        {
            return CreateJSONObject(map);
        }

        private static SimpleJSON.JSONNode CreateJSONObject(GameObject gameObject)
        {
            SimpleJSON.JSONNode node = new SimpleJSON.JSONObject();
            Transform transform = gameObject.GetComponent<Transform>();
            node.Add("name", gameObject.name);
            node.Add("position", SimpleJSON.JSON.Parse(JsonUtility.ToJson(transform.localPosition)));
            node.Add("rotation", SimpleJSON.JSON.Parse(JsonUtility.ToJson(transform.localRotation)));
            node.Add("scale", SimpleJSON.JSON.Parse(JsonUtility.ToJson(transform.localScale)));
            node.Add("tag", gameObject.tag);
            node.Add("layer", gameObject.layer);

            if (gameObject.tag.Contains("BuildingPrefab"))
                SerializeBuildingPrefab(gameObject, ref node);
            else if (gameObject.tag.Contains("Terrain"))
                SerializeTerrain(gameObject, ref node);
            else if (gameObject.tag.Contains("JumpPadPrefab"))
                SerializeJumpPadPrefab(gameObject, ref node);
            else if (gameObject.tag.Contains("Ground"))
                SerializeGround(gameObject, ref node);
            else
                SerializeChildren(gameObject, ref node);

            return node;
        }

        private static void SerializeBuildingPrefab(GameObject buildingPrefab, ref SimpleJSON.JSONNode json)
        {
            string name = buildingPrefab.name;
            json.Add("prefab", name.Substring(0, name.ToLower().LastIndexOf("(clone)")));
        }

        private static void SerializeTerrain(GameObject map, ref SimpleJSON.JSONNode json)
        {
            MeshFilter meshFilter = map.GetComponent<MeshFilter>();
            json.Add("terrain", TERRAIN_MESH_FILE);
            SerializeChildren(map, ref json);
        }

        private static void SerializeJumpPadPrefab(GameObject jumpPadPrefab, ref SimpleJSON.JSONNode json)
        {
            json.Add("prefab", "jumpPad");
            json.Add("properties", SimpleJSON.JSON.Parse(JsonUtility.ToJson(jumpPadPrefab.GetComponentInChildren<JumpPad>().jumpPadProperties)));
        }

        private static void SerializeGround(GameObject ground, ref SimpleJSON.JSONNode json)
        {
            json.Add("prefab", "ground");
            json.Add("properties", SimpleJSON.JSON.Parse(JsonUtility.ToJson(ground.GetComponent<Ground>().GroundProperties)));
        }

        private static void SerializeChildren(GameObject gameObject, ref SimpleJSON.JSONNode json)
        {
            SimpleJSON.JSONNode children = new SimpleJSON.JSONArray();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                children.Add(child.name, CreateJSONObject(child));
            }

            if (gameObject.transform.childCount > 0)
                json.Add("children", children);
        }

        public static SimpleJSON.JSONObject SerializeSpawnGoals(List<Vector3[]> spawnsGoals)
        {
            SimpleJSON.JSONObject json = new SimpleJSON.JSONObject();
            SimpleJSON.JSONArray array = new SimpleJSON.JSONArray();
            foreach (Vector3[] spawnGoal in spawnsGoals)
            {
                SimpleJSON.JSONArray spawnGoalJSON = new SimpleJSON.JSONArray();
                spawnGoalJSON.Add(SimpleJSON.JSON.Parse(JsonUtility.ToJson(spawnGoal[0])));
                spawnGoalJSON.Add(SimpleJSON.JSON.Parse(JsonUtility.ToJson(spawnGoal[1])));
                array.Add(spawnGoalJSON);
            }
            json.Add("spawnsGoals", array);
            return json;
        }

        /// <summary>
        /// Load a map into a gameobject from either the main map json or the config json file.
        /// </summary>
        /// <param name="mapFolder">Path to map folder</param>
        /// <returns>Gameobject containing the map.</returns>
        public static GameObject LoadMap(string mapFolder)
        {
            if (!System.IO.Directory.Exists(mapFolder))
                return null;

            GameObject map = new GameObject();
            LoadMap(ref map, mapFolder);
            return map;
        }

        private static void LoadMap(ref GameObject map, string mapFolder)
        {
            SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(mapFolder, MAP_FILE)));
            map.name = json["name"];
            LoadMap(ref map, json, mapFolder);
        }

        private static void LoadMap(ref GameObject map, SimpleJSON.JSONNode json, string mapFolder)
        {
            foreach (SimpleJSON.JSONNode node in json["children"])
                Load(ref map, node, mapFolder);
        }

        private static void Load(ref GameObject gameObject, SimpleJSON.JSONNode json, string mapFolder)
        {
            GameObject newGO;

            if (json["prefab"] != null)
                LoadPrefab(json, out newGO, mapFolder);
            else
                newGO = new GameObject();

            newGO.transform.parent = gameObject.transform;

            if (json["terrain"] != null)
                LoadTerrain(ref newGO, json, mapFolder);

            if (json["children"] != null)
            {
                foreach (SimpleJSON.JSONNode child in json["children"])
                    Load(ref newGO, child, mapFolder);
            }

            newGO.name = json["name"];
            newGO.transform.localPosition = JsonUtility.FromJson<Vector3>(json["position"].ToString());
            newGO.transform.localRotation = JsonUtility.FromJson<Quaternion>(json["rotation"].ToString());
            newGO.transform.localScale = JsonUtility.FromJson<Vector3>(json["scale"].ToString());
            newGO.tag = json["tag"];
            newGO.layer = json["layer"];
        }

        private static void LoadPrefab(SimpleJSON.JSONNode json, out GameObject gameObject, string mapFolder)
        {
            if (json["prefab"] == "jumpPad")
                LoadJumpPadPrefab(json, out gameObject);
            else if (json["prefab"] == "ground")
                LoadGround(json, out gameObject, mapFolder);
            else
                LoadBuildingPrefab(json, out gameObject);
        }

        private static void LoadJumpPadPrefab(SimpleJSON.JSONNode json, out GameObject gameObject)
        {
            Object prefab = Resources.Load("Prefabs/" + json["prefab"]);
            gameObject = Object.Instantiate(prefab) as GameObject;
            JsonUtility.FromJsonOverwrite(json["properties"].ToString(), gameObject.GetComponentInChildren<JumpPad>().jumpPadProperties);
        }

        private static List<GameObject> prefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs").ToList();
        private static void LoadBuildingPrefab(SimpleJSON.JSONNode json, out GameObject gameObject)
        {
            GameObject prefab = prefabs.Find((GameObject prefab) => prefab.name == json["prefab"]);
            gameObject = Object.Instantiate(prefab);
        }

        private static void LoadGround(SimpleJSON.JSONNode json, out GameObject gameObject, string mapFolder)
        {
            string path = System.IO.Path.Combine(mapFolder, "grounds", json["name"] + ".bin");
            gameObject = Ground.Load(path);
            Ground ground = gameObject.GetComponent<Ground>();
            JsonUtility.FromJsonOverwrite(json["properties"].ToString(), ground.GroundProperties);
            ground.UpdateMaterial();
        }

        private static void LoadTerrain(ref GameObject gameObject, SimpleJSON.JSONNode json, string mapFolder)
        {
            string path = System.IO.Path.Combine(mapFolder, TERRAIN_MESH_FILE);
            TerrainLoader terrainLoader = gameObject.AddComponent<TerrainLoader>();
            terrainLoader.LoadTerrain(path);
        }

        /// <summary>
        /// Load spawn-goal pairs into a list of Vector3 from the map folder.
        /// </summary>
        /// <param name="mapFolder">Path to the map folder.</param>
        /// <returns>List of pairs of Vector3. The first one corresponds to the spawn point and the second one is the goal.</returns>
        public static List<Vector3[]> LoadSpawnGoals(string mapFolder)
        {
            mapFolder = System.IO.Path.Combine(mapFolder, MapSerializer.SPAWN_GOALS_FILE);
            if (!System.IO.File.Exists(mapFolder))
                return null;
            SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(System.IO.File.ReadAllText(mapFolder));
            return LoadSpawnGoals(json);
        }

        private static List<Vector3[]> LoadSpawnGoals(SimpleJSON.JSONNode json)
        {
            List<Vector3[]> spawnGoals = new List<Vector3[]>();
            SimpleJSON.JSONNode array = json["spawnsGoals"];
            if (array == null) return null;
            foreach (SimpleJSON.JSONNode spawnGoalJSON in array.Children)
            {
                Vector3[] spawnGoal = new Vector3[2];
                spawnGoal[0] = JsonUtility.FromJson<Vector3>(spawnGoalJSON[0].ToString());
                spawnGoal[1] = JsonUtility.FromJson<Vector3>(spawnGoalJSON[1].ToString());
                spawnGoals.Add(spawnGoal);
            }

            return spawnGoals;
        }

        public struct SpawnGoalAndMap
        {
            public SpawnGoalAndMap(List<Vector3[]> spawnsGoals, GameObject map)
            {
                SpawnsGoals = spawnsGoals;
                Map = map;
            }
            public List<Vector3[]> SpawnsGoals;
            public GameObject Map;
        }

        /// <summary>
        /// Load spawn-goal pairs and the map.
        /// </summary>
        /// <param name="mapFolder">Path to the map folder.</param>
        /// <returns>SpawnGoalAndMap is a struct containing the list of Vector3 for the spawn-goal pairs and the gameobject in which the map has been loaded.</returns>
        public static SpawnGoalAndMap? LoadSpawnGoalsAndMap(string mapFolder)
        {
            if (!System.IO.Directory.Exists(mapFolder))
                return null;
            List<Vector3[]> spawnGoals = LoadSpawnGoals(mapFolder);
            GameObject map = LoadMap(mapFolder);
            return new SpawnGoalAndMap(spawnGoals, map);
        }
    }
}
