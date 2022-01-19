using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

public class BuildingGeneratorTests : TestBase
{
    private TerrainGenerator terrainGenerator;
    private BuildingGenerator buildingGenerator;
    private GameObject map;

    [SetUp]
    public void SetUp()
    {
        GameObject mapGenerator = new GameObject();
        terrainGenerator = mapGenerator.AddComponent<TerrainGenerator>();

        terrainGenerator.Volume = new Vector3(5, 5, 5);
        terrainGenerator.Scale = 1;

        buildingGenerator = mapGenerator.AddComponent<BuildingGenerator>();
        buildingGenerator.GenerateBuildings = true;
        buildingGenerator.PlatformGenerationStep.MinNbPlatforms = 1;
        buildingGenerator.PlatformGenerationStep.MaxNbPlatforms = 1;
        buildingGenerator.PlatformGenerationStep.MinHeight = 0;
        buildingGenerator.PlatformGenerationStep.MaxHeight = 0;
        buildingGenerator.PlatformGenerationStep.InnerRadius = 1.5f;
        buildingGenerator.PlatformGenerationStep.OuterRadius = 1.6f;
        buildingGenerator.nbOfFloors = 2;

        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("TestPrefab/BuildingMap"));
        map = GameObject.FindGameObjectWithTag("Map");
    }

    [UnityTest]
    public IEnumerator BuildingTest()
    {
        terrainGenerator.Generate();
        System.Random randomInt = new System.Random(terrainGenerator.seed);
        buildingGenerator.Generate(randomInt.Next(), scale: terrainGenerator.Scale);
        yield return null;
        TestUtils.AssertGameObjectHierarchyEqual(buildingGenerator.BuildingsGO.First(), map.transform.GetChild(1).gameObject);
    }

    [Test]
    public void MeshGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        System.Random randomInt = new System.Random(terrainGenerator.seed);
        buildingGenerator.Generate(randomInt.Next(), scale: terrainGenerator.Scale);
        Mesh mesh = terrainGenerator.ComputeMesh(grid);

        TestUtils.AssertMeshEqual(mesh, map.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
    }

    [UnityTest]
    public IEnumerator TooManyNonOverlappingBuildingsTest()
    {
        buildingGenerator.PlatformGenerationStep.MinNbPlatforms = 2;
        buildingGenerator.PlatformGenerationStep.MaxNbPlatforms = 2;
        terrainGenerator.Generate();
        buildingGenerator.Generate(scale: terrainGenerator.Scale);
        yield return null;
        Assert.That(buildingGenerator.BuildingsGO.Count, Is.EqualTo(1), "Should contain only one building.");
        LogAssert.Expect(LogType.Warning, "Unable to fit 2 non-overlapping platforms in the map, settling for 1.");
    }

    [UnityTest]
    public IEnumerator ManyOverlappingBuildingsTest()
    {
        buildingGenerator.PlatformGenerationStep.MinNbPlatforms = 2;
        buildingGenerator.PlatformGenerationStep.MaxNbPlatforms = 2;
        buildingGenerator.PlatformGenerationStep.NonOverlapping = false;
        terrainGenerator.Generate();
        buildingGenerator.Generate(scale: terrainGenerator.Scale);
        yield return null;
        Assert.That(buildingGenerator.BuildingsGO.Count, Is.EqualTo(2), "Should contain two buildings.");
    }
}
