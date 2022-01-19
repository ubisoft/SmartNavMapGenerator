using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;

public class GroundGenerator : MonoBehaviour
{
    public bool GenerateGrounds = false;

    [Range(0, 1)]
    public float LavaProbability = 0.5f;
    private float _oldLavaProbability = 0.5f;

    [Range(0, 1)]
    public float WaterProbability = 0.5f;
    private float _oldWaterProbability = 0.5f;

    public int DebugSeed = -1;
    private System.Random _rng;

    private void OnEnable()
    {
    }

    private void OnValidate()
    {
        if (Mathf.Abs(LavaProbability - _oldLavaProbability) > 0)
        {
            WaterProbability += _oldLavaProbability - LavaProbability;
            WaterProbability = Mathf.Clamp01(WaterProbability);
        }
        else if (Mathf.Abs(WaterProbability - _oldWaterProbability) > 0)
        {
            LavaProbability += _oldWaterProbability - WaterProbability;
            LavaProbability = Mathf.Clamp01(LavaProbability);
        }

        _oldLavaProbability = LavaProbability;
        _oldWaterProbability = WaterProbability;
    }

    private GameObject groundsGO;

    public GameObject GroundsGO { get => groundsGO; }

    public void Generate(NoiseGrid grid, float hardFloor, int seed = 0, GameObject parent = null)
    {
        if (!GenerateGrounds) return;
        _rng = new System.Random(DebugSeed < 0 ? seed : DebugSeed);
        NoiseGrid subGrid = grid.SubGrid(1, 0, 1, -2, Mathf.FloorToInt(hardFloor / grid.Spacing.y), -2);

        List<Mesh> meshes = new List<Mesh>();
        MarchingCube marchingCube = new MarchingCube();
        int padding = 2;
        NoiseGrid paddedGrid = NoiseGrid.EvenPaddedGrid(subGrid, padding);
        paddedGrid.ForEach((gridPoint, gridPos) =>
        {
            gridPoint.Value = -Mathf.Abs(gridPoint.Value);
        });
        subGrid.ForEach((gridPoint, gridPos) =>
        {
            if (gridPoint.Value < 0 && gridPoint.Value > -40)
            {
                NoiseGrid paddedExtract = new NoiseGrid(paddedGrid);
                FloodFillExtract(gridPos, ref subGrid, ref paddedExtract, padding);

                Mesh mesh = marchingCube.ComputeMesh(paddedExtract, subGrid.Isovalue, subGrid.Volume);
                meshes.Add(mesh);
            }
        });

        if (!groundsGO)
            DestroyImmediate(groundsGO);
        groundsGO = new GameObject("Grounds");
        groundsGO.transform.parent = parent?.transform;
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].name = $"Ground{i}";
            GameObject ground = WrapMeshInGameObject(meshes[i].name, meshes[i]);
            ground.transform.parent = groundsGO.transform;
            SetGroundType(ground);
        }
    }

    private void FloodFillExtract(in Vector3Int point, ref NoiseGrid source, ref NoiseGrid destination, in int padding = 0)
    {
        Stack<Vector3Int> gridPointCoordinates = new Stack<Vector3Int>();
        gridPointCoordinates.Push(point);
        Vector3Int paddings = Vector3Int.one * padding / 2;
        while (gridPointCoordinates.Count > 0)
        {
            Vector3Int a = gridPointCoordinates.Pop();
            if (a.x < source.GetLength(0) && a.x >= 0 &&
                    a.y < source.GetLength(1) && a.y >= 0 && a.z < source.GetLength(2) && a.z >= 0)//make sure we stay within bounds
            {
                GridPoint gridPoint = source[a];
                if (gridPoint.Value < 0 && gridPoint.Value > -40)
                {
                    destination[a + paddings].Value = -gridPoint.Value;
                    gridPoint.Value = -gridPoint.Value;
                    gridPointCoordinates.Push(new Vector3Int(a.x - 1, a.y, a.z));
                    gridPointCoordinates.Push(new Vector3Int(a.x + 1, a.y, a.z));
                    gridPointCoordinates.Push(new Vector3Int(a.x, a.y - 1, a.z));
                    gridPointCoordinates.Push(new Vector3Int(a.x, a.y + 1, a.z));
                    gridPointCoordinates.Push(new Vector3Int(a.x, a.y, a.z - 1));
                    gridPointCoordinates.Push(new Vector3Int(a.x, a.y, a.z + 1));
                }
            }
        }
    }

    private GameObject WrapMeshInGameObject(string name, Mesh mesh)
    {
        GameObject gameObject = Ground.CreateGameObject(mesh);
        gameObject.name = name;
        gameObject.transform.position += Vector3.down * (0.025f + 0.15f);
        return gameObject;
    }

    private void SetGroundType(GameObject groundGO)
    {
        Ground ground = groundGO.GetComponent<Ground>();
        float[] weights = new float[] { LavaProbability, WaterProbability };
        GroundProperties.GroundType type = (GroundProperties.GroundType)_rng.GetRandomWeightedIndex(weights);
        ground.GroundProperties = new GroundProperties(type);
    }
}
