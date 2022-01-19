using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using LaForge.MapGenerator;

public class LoadingTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        Resources.Load<GameObject>("Prefabs/JumpPad").GetComponent<Animator>().enabled = false;
    }

    static string[] mapNames = new string[] { "BaseMap", "BuildingMap", "GroundMap", "JumpPadMap" };
    [UnityTest]
    public IEnumerator LoadingTest([ValueSource("mapNames")] string mapName)
    {
        GameObject map = MapSerializer.LoadMap(Path.Combine("Assets/Tests/Maps/", mapName, mapName));
        GameObject prefab = Object.Instantiate(Resources.Load<GameObject>(Path.Combine("TestPrefab", mapName)));
        GameObject mapExpected = prefab.transform.GetChild(1).gameObject;
        yield return null;

        TestUtils.AssertGameObjectHierarchyEqual(map, mapExpected);
    }

    [Test]
    public void SpawnGoalLoadTest()
    {
        List<Vector3[]> spawnGoals = MapSerializer.LoadSpawnGoals("Assets/Tests/Maps/GroundMapWithGoals/GroundMapWithGoals");
        GameObject map = Object.Instantiate(Resources.Load<GameObject>(Path.Combine("TestPrefab", "GroundMapWithGoals")));
        Vector3[] expectedSpawnGoal = map.GetComponentInChildren<SerializeSpawnGoals>().spawnGoal;

        Assert.That(spawnGoals[0], Is.EqualTo(expectedSpawnGoal));
    }
}
