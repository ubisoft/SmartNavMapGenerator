using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LaForge.MapGenerator;

public class GroundGeneratorTests : TestBase
{
    private TerrainGenerator terrainGenerator;
    private GroundGenerator groundGenerator;
    private GameObject map;

    [SetUp]
    public void SetUp()
    {
        GameObject mapGenerator = new GameObject();
        terrainGenerator = mapGenerator.AddComponent<TerrainGenerator>();

        terrainGenerator.Volume = new Vector3(5, 5, 5);
        terrainGenerator.Scale = 1;
        terrainGenerator.HardFloorWeight = 2;

        groundGenerator = mapGenerator.AddComponent<GroundGenerator>();
        groundGenerator.GenerateGrounds = true;

        Resources.Load<GameObject>("Prefabs/JumpPad").GetComponent<Animator>().enabled = false;

        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("TestPrefab/GroundMap"));
        map = GameObject.FindGameObjectWithTag("Map");
    }

    [UnityTest]
    public IEnumerator GroundGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        groundGenerator.Generate(grid, terrainGenerator.HardFloor, terrainGenerator.seed);
        yield return null;
        GameObject expectedParentGround = map.transform.GetChild(0).GetChild(0).gameObject;
        TestUtils.AssertGameObjectHierarchyEqual(groundGenerator.GroundsGO, expectedParentGround, assertMeshEqual: true);

        Ground[] grounds = groundGenerator.GroundsGO.GetComponentsInChildren<Ground>();
        Ground[] expectedGrounds = expectedParentGround.GetComponentsInChildren<Ground>();
        for (int i = 0; i < grounds.Length; i++)
        {
            Assert.That(grounds[i].GroundProperties, Is.EqualTo(expectedGrounds[i].GroundProperties));
        }
    }

    [Test]
    public void MeshGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        groundGenerator.Generate(grid, terrainGenerator.HardFloor, terrainGenerator.seed);
        Mesh mesh = terrainGenerator.ComputeMesh(grid);

        TestUtils.AssertMeshEqual(mesh, map.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
    }
}
