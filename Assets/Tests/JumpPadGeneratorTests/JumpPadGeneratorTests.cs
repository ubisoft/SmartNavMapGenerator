using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using LaForge.MapGenerator;

public class JumpPadGeneratorTests : TestBase
{
    private TerrainGenerator terrainGenerator;
    private JumppadGenerator jumppadGenerator;
    private GameObject map;

    [SetUp]
    public void SetUp()
    {
        GameObject mapGenerator = new GameObject();
        terrainGenerator = mapGenerator.AddComponent<TerrainGenerator>();

        terrainGenerator.Volume = new Vector3(5, 5, 5);
        terrainGenerator.Scale = 6;

        jumppadGenerator = mapGenerator.AddComponent<JumppadGenerator>();
        jumppadGenerator.GenerateJumpPads = true;
        jumppadGenerator.SteepnessThreshold = 4;
        jumppadGenerator.MinNbJumpPads = 7;
        jumppadGenerator.MinNbJumpPads = 7;

        Resources.Load<GameObject>("Prefabs/JumpPad").GetComponent<Animator>().enabled = false;

        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("TestPrefab/JumpPadMap"));
        map = GameObject.FindGameObjectWithTag("Map");
        map.GetComponentsInChildren<Animator>().ToList().ForEach(animator => animator.enabled = false);
    }

    [TearDown]
    public void TearDown()
    {
        Resources.Load<GameObject>("Prefabs/JumpPad").GetComponent<Animator>().enabled = true;
    }

    [UnityTest]
    public IEnumerator JumpPadGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        Vector3[,] heightMap = HeightMap.WorldBasedHeightMap(grid, terrainGenerator.HardFloor - grid.Spacing.y);
        jumppadGenerator.Generate(heightMap, grid, terrainGenerator.seed);
        jumppadGenerator.JumpPadsGO.GetComponentsInChildren<Animator>().ToList().ForEach(animator => animator.enabled = false);
        yield return null;
        TestUtils.AssertGameObjectHierarchyEqual(jumppadGenerator.JumpPadsGO, map.transform.GetChild(1).gameObject);

        JumpPad[] jumpPads = jumppadGenerator.JumpPadsGO.GetComponentsInChildren<JumpPad>();
        JumpPad[] expectedJumppads = map.transform.GetChild(1).gameObject.GetComponentsInChildren<JumpPad>();
        for (int i = 0; i < expectedJumppads.Length; i++)
        {
            Assert.That(jumpPads[i].jumpPadProperties, Is.EqualTo(expectedJumppads[i].jumpPadProperties));
        }
    }

    [UnityTest]
    public IEnumerator TooManyJumpPadGenerationTest()
    {
        jumppadGenerator.MinNbJumpPads = 100;
        jumppadGenerator.MaxNbJumpPads = 100;
        NoiseGrid grid = terrainGenerator.Generate();
        Vector3[,] heightMap = HeightMap.WorldBasedHeightMap(grid, terrainGenerator.HardFloor - grid.Spacing.y);
        jumppadGenerator.Generate(heightMap, grid, terrainGenerator.seed);
        jumppadGenerator.JumpPadsGO.GetComponentsInChildren<Animator>().ToList().ForEach(animator => animator.enabled = false); 
        yield return null;
        Assert.That(jumppadGenerator.JumpPadsGO.transform.childCount, Is.EqualTo(7));
    }

    [Test]
    public void MeshGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        Vector3[,] heightMap = HeightMap.WorldBasedHeightMap(grid, terrainGenerator.HardFloor - grid.Spacing.y);
        jumppadGenerator.Generate(heightMap, grid, terrainGenerator.seed);
        Mesh mesh = terrainGenerator.ComputeMesh(grid);

        TestUtils.AssertMeshEqual(mesh, map.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
    }
}
