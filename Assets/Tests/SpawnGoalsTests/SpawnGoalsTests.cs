using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using LaForge.MapGenerator;

public class SpawnGoalsTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
    }


    static string[] mapNames = new string[] { "GroundMapWithGoals", "GroundMapWithUniformGoals" };
    [UnityTest]
    public IEnumerator SpawnGoalsTest([ValueSource("mapNames")] string mapName)
    {
        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>(Path.Combine("TestPrefab", mapName)));
        Random.InitState(GameObject.FindObjectOfType<TerrainGenerator>().seed);
        SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
        spawnGoalGenerator.Clear();
        spawnGoalGenerator.GenerateNavMesh();
        yield return null;

        List<Vector3[]> spawnGoals = spawnGoalGenerator.GeneralGoals();
        Assert.That(spawnGoals[0], Is.EqualTo(GameObject.FindObjectOfType<SerializeSpawnGoals>().spawnGoal));
    }

    [UnityTest]
    public IEnumerator Validate500SpawnGoalsTest()
    {
        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("TestPrefab/GroundMapWithGoals"));
        Random.InitState(GameObject.FindObjectOfType<TerrainGenerator>().seed);
        SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
        spawnGoalGenerator.Clear();
        spawnGoalGenerator.GenerateNavMesh();
        yield return null;

        spawnGoalGenerator.TotalGoals = 500;

        List<Vector3[]> spawnGoals = spawnGoalGenerator.GeneralGoals();
        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        Bounds groundBounds = ground.GetComponent<BoxCollider>().bounds;

        for (int i = 0; i < spawnGoals.Count; i++)
        {
            Assert.That(groundBounds.Contains(spawnGoals[i][0]), Is.False, "Spawn #" + i + ": " + spawnGoals[i][0] + " shouldn't be inside the collider " + groundBounds + ".");
            Assert.That(groundBounds.Contains(spawnGoals[i][1]), Is.False, "Goal #" + i + ": " + spawnGoals[i][1] + " shouldn't be inside the collider " + groundBounds + ".");
        }
    }
}
