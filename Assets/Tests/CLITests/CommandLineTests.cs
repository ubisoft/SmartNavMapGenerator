using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using LaForge.MapGenerator;

public class CommandLineTests : TestBase
{
    string _tmpPath;
    [SetUp]
    public void SetUp()
    {
        _tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tmpPath);

        GameObject mapGeneratorGO = new GameObject("MapGenerator");
        MapGenerator mapGenerator = mapGeneratorGO.AddComponent<MapGenerator>();
        mapGenerator.TerrainGenerator = mapGeneratorGO.AddComponent<TerrainGenerator>();
        mapGenerator.BuildingGenerator = mapGeneratorGO.AddComponent<BuildingGenerator>();
        mapGenerator.JumppadGenerator = mapGeneratorGO.AddComponent<JumppadGenerator>();
        mapGenerator.GroundGenerator = mapGeneratorGO.AddComponent<GroundGenerator>();
        mapGeneratorGO.AddComponent<SpawnGoalGenerator>();
    }

    [TearDown]
    public void RemoveTmpFolder()
    {
        Directory.Delete(_tmpPath, true);
    }

    static Dictionary<string,string> args = new Dictionary<string, string> {
        { "BaseMap", "-- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False --name BaseMap" },
        { "BuildingMap", "-- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False --buildings --min-buildings 1 --max-buildings 1 --nb-floors 2 --platform-inner-radius 1.5 --platform-outer-radius 1.6 --platform-min-height 0 --platform-max-height 0 --name BuildingMap" },
        { "GroundMap", "-- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False --hard-floor-weight 2 --grounds --name GroundMap" },
        { "GroundMapWithGoals", "-- generate --seed 0 --nb-maps=1 --scale 6 --volume 5,5,5 --jumppads=False --hard-floor-weight 2 --grounds --spawn-goals 1 --name GroundMapWithGoals" },
        { "JumpPadMap", "-- generate --seed 0 --nb-maps=1 --scale 6 --volume 5,5,5 --min-jumppads 7 --max-jumppads 7 --steepness 4 --name JumpPadMap" } 
    };
    static string[] mapNames = new string[] { "BaseMap", "BuildingMap", "GroundMap", "GroundMapWithGoals", "JumpPadMap" };
    [UnityTest]
    public IEnumerator GenerateTest([ValueSource("mapNames")] string mapName)
    {
        string[] argArray = (args[mapName] + $" --output {_tmpPath}").Split();
        yield return CommandLine.Instance.Coroutine_Execute(argArray);

        string jsonConfig = File.ReadAllText(Path.Combine(_tmpPath, mapName, MapSerializer.CONFIG_FILE));
        string jsonConfigExpected = File.ReadAllText(Path.Combine("Assets/Tests/Maps", mapName, mapName, MapSerializer.CONFIG_FILE));

        Assert.That(jsonConfig, Is.EqualTo(jsonConfigExpected));

        byte[] meshBytes = File.ReadAllBytes(Path.Combine(_tmpPath, mapName, MapSerializer.TERRAIN_MESH_FILE));
        byte[] meshBytesExpected = File.ReadAllBytes(Path.Combine("Assets/Tests/Maps", mapName, mapName, MapSerializer.TERRAIN_MESH_FILE));

        Assert.That(meshBytes, Is.EqualTo(meshBytesExpected));

        if (mapName.Contains("Goals"))
        {
            string jsonStr = File.ReadAllText(Path.Combine(_tmpPath, mapName, MapSerializer.SPAWN_GOALS_FILE));
            string expectedJsonStr = File.ReadAllText(Path.Combine("Assets/Tests/Maps/GroundMapWithGoals/GroundMapWithGoals", MapSerializer.SPAWN_GOALS_FILE));
            
            Assert.That(jsonStr, Is.EqualTo(expectedJsonStr));
        }
    }
}
