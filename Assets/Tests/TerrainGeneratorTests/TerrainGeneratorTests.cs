using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

public class TerrainGeneratorTests : TestBase
{
    private TerrainGenerator terrainGenerator;
    private GameObject map;

    [SetUp]
    public void SetUp()
    {
        GameObject mapGenerator = new GameObject();
        terrainGenerator = mapGenerator.AddComponent<TerrainGenerator>();

        terrainGenerator.Volume = new Vector3(5, 5, 5);
        terrainGenerator.Scale = 1;

        GameObject prefab = GameObject.Instantiate(Resources.Load<GameObject>("TestPrefab/BaseMap"));
        map = GameObject.FindGameObjectWithTag("Map");
    }

    [Test]
    public void MeshGenerationTest()
    {
        NoiseGrid grid = terrainGenerator.Generate();
        Mesh mesh = terrainGenerator.ComputeMesh(grid);

        TestUtils.AssertMeshEqual(mesh, map.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
    }
}
