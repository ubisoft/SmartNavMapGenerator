using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.IO;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

public class SerializationTests : TestBase
{
    private string _tmpPath;

    [SetUp]
    public void CreateTmpFolder()
    {
        _tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tmpPath);
    }

    [TearDown]
    public void RemoveTmpFolder()
    {
        Directory.Delete(_tmpPath, true);
    }

    [TestCase("BaseMap")]
    [TestCase("BuildingMap")]
    [TestCase("GroundMap")]
    [TestCase("JumpPadMap")]
    public void SerializeTest(string prefabPath)
    {
        GameObject prefab = Object.Instantiate(Resources.Load<GameObject>(Path.Combine("TestPrefab", prefabPath)));
        
        MapGenerator mapGenerator = GameObject.FindObjectOfType<MapGenerator>();
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        MapGenerator.SaveMap(_tmpPath, false, map,
        map.GetComponentInChildren<TerrainLoader>(),
        mapGenerator.GetComponent<TerrainGenerator>(),
        mapGenerator.GetComponent<BuildingGenerator>(),
        mapGenerator.GetComponent<JumppadGenerator>(),
        mapGenerator.GetComponent<GroundGenerator>());

        string jsonConfig = File.ReadAllText(Path.Combine(_tmpPath, MapSerializer.CONFIG_FILE));
        string jsonConfigExpected = File.ReadAllText(Path.Combine("Assets/Tests/Maps", map.name, map.name, MapSerializer.CONFIG_FILE));

        Assert.That(jsonConfig, Is.EqualTo(jsonConfigExpected));

        byte[] meshBytes = File.ReadAllBytes(Path.Combine(_tmpPath, MapSerializer.TERRAIN_MESH_FILE));
        byte[] meshBytesExpected = File.ReadAllBytes(Path.Combine("Assets/Tests/Maps", map.name, map.name, MapSerializer.TERRAIN_MESH_FILE));

        Assert.That(meshBytes, Is.EqualTo(meshBytesExpected));
    }

    [Test]
    public void SpawnGoalsSerializeTest()
    {
        GameObject prefab = Object.Instantiate(Resources.Load<GameObject>(Path.Combine("TestPrefab", "GroundMapWithGoals")));

        JSONObject json = MapSerializer.SerializeSpawnGoals(new List<Vector3[]> { GameObject.FindObjectOfType<SerializeSpawnGoals>().spawnGoal });
        File.WriteAllText(Path.Combine(_tmpPath, MapSerializer.SPAWN_GOALS_FILE), json.ToString(4));

        string jsonStr = File.ReadAllText(Path.Combine(_tmpPath, MapSerializer.SPAWN_GOALS_FILE));
        string expectedJsonStr = File.ReadAllText(Path.Combine("Assets/Tests/Maps/GroundMapWithGoals/GroundMapWithGoals", MapSerializer.SPAWN_GOALS_FILE));
        Assert.That(jsonStr, Is.EqualTo(expectedJsonStr));
    }
}
