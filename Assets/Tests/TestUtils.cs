using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;

public static class TestUtils
{
    public static void AssertGameObjectHierarchyEqual(GameObject gameObject, GameObject expected, bool assertMeshEqual = false)
    {
        AssertGameObjectEqual(gameObject, expected, assertMeshEqual);
        Assert.That(gameObject.transform.childCount, Is.EqualTo(expected.transform.childCount), "Child count : " + gameObject.name + " - " + expected.name);
        for (int i = 0; i < expected.transform.childCount; i++)
        {
            AssertGameObjectHierarchyEqual(gameObject.transform.GetChild(i).gameObject, expected.transform.GetChild(i).gameObject);
        }
    }

    public static void AssertGameObjectEqual(GameObject gameObject, GameObject expected, bool assertMeshEqual = false)
    {
        Assert.That(gameObject.transform.localPosition, Is.EqualTo(expected.transform.localPosition), "Local position : " + gameObject.name + " - " + expected.name);
        Assert.That(gameObject.transform.localRotation, Is.EqualTo(expected.transform.localRotation), "Local rotation : " + gameObject.name + " - " + expected.name);
        Assert.That(gameObject.transform.localScale, Is.EqualTo(expected.transform.localScale), "Local scale : " + gameObject.name + " - " + expected.name);


        MeshFilter meshFilterExpected = expected.GetComponent<MeshFilter>();
        if (meshFilterExpected != null)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            Assert.That(meshFilter, Is.Not.Null, "Mesh filter : " + gameObject.name + " - " + expected.name);
            Assert.That(meshFilter.name, Is.EqualTo(meshFilterExpected.name), "Mesh name : " + gameObject.name + " - " + expected.name);
            if (assertMeshEqual)
            {
                AssertMeshEqual(meshFilter.sharedMesh, meshFilterExpected.sharedMesh);
            }
        }

        Collider colliderExpected = expected.GetComponent<Collider>();
        if (colliderExpected != null)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null, "Collider : " + gameObject.name + " - " + expected.name);
            Assert.That(collider.bounds, Is.EqualTo(colliderExpected.bounds), "Collider bounds : " + gameObject.name + " - " + expected.name);
        }
    }

    public static void AssertMeshEqual(Mesh mesh, Mesh expected)
    {
        Assert.That(mesh.vertices, Is.EqualTo(expected.vertices), "Mesh vertices : " + mesh.name + " - " + expected.name);
        Assert.That(mesh.triangles, Is.EqualTo(expected.triangles), "Mesh triangles : " + mesh.name + " - " + expected.name);
        Assert.That(mesh.uv, Is.EqualTo(expected.uv), "Mesh uv : " + mesh.name + " - " + expected.name);
        Assert.That(mesh.uv2, Is.EqualTo(expected.uv2), "Mesh uv2 : " + mesh.name + " - " + expected.name);
    }
}
