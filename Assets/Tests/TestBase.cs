using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

public class TestBase
{
    [TearDown]
    public void TearDownBase()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>())
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}