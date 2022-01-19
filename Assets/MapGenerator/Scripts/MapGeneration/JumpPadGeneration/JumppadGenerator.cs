using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;

public class JumppadGenerator : MonoBehaviour
{
    public bool GenerateJumpPads = true;
    public float SteepnessThreshold = 5;
    public float MinFlatness = -0.5f;
    public float SearchRadius = 5;
    public float ClusterRadius = 5;
    public int MinNbJumpPads = 10;
    public int MaxNbJumpPads = 10;
    public int DebugSeed = -1;
    public bool DebugDisplay = false;
    private System.Random rng;
    private GameObject jumpPadPrefab;

    private void Awake()
    {
        jumpPadPrefab = Resources.Load<GameObject>("Prefabs/jumpPad");
    }

# if UNITY_EDITOR
    private GameObject heightMap;
# endif
    private GameObject jumpPadsGO;

    public GameObject JumpPadsGO { get => jumpPadsGO; }

    public void Generate(in Vector3[,] worldBasedHeightMap, NoiseGrid grid, in int seed = 0, GameObject parent = null)
    {
        if (!GenerateJumpPads) return;
        rng = new System.Random(DebugSeed < 0 ? seed : DebugSeed);
        if (jumpPadsGO != null)
            DestroyImmediate(jumpPadsGO);
        List<JumpPadProperties> validJumpPads = ScanHeightMap(worldBasedHeightMap, grid.Spacing, grid.Scale);
# if UNITY_EDITOR
        if (DebugDisplay)
        {
            if (heightMap != null)
                DestroyImmediate(heightMap);
            heightMap = new GameObject("heightmap");
            foreach (JumpPadProperties jumpPadProperties in validJumpPads)
            {
                GameObject height = GameObject.CreatePrimitive(PrimitiveType.Cube);
                height.transform.localScale = Vector3.one * 0.05f;
                height.transform.position = jumpPadProperties.Position;
                height.GetComponent<Renderer>().material.color = Color.blue;
                height.transform.parent = heightMap.transform;
            }
        }
# endif
        List<Cluster> clusters = ClusterPotentialJumpPads(validJumpPads);
        List<JumpPadProperties> jumpPads = SampleJumpPads(clusters);

        jumpPadsGO = new GameObject("Jump Pads");
        jumpPadsGO.isStatic = true;
        jumpPadsGO.transform.parent = parent?.transform;
        Physics.autoSyncTransforms = true;
        int i = 0;
        foreach (JumpPadProperties jumpPad in jumpPads)
        {
            GameObject jumppadGO = Instantiate(jumpPadPrefab, jumpPadsGO.transform);
            jumppadGO.name += $"_{i}_{jumpPad.Height}";
            jumppadGO.GetComponentInChildren<JumpPad>().jumpPadProperties = jumpPad;
            jumppadGO.transform.position = jumpPad.Position;
            CarveGrid(jumppadGO, grid);
            i++;
        }
        Physics.autoSyncTransforms = false;
    }

    private List<JumpPadProperties> ScanHeightMap(in Vector3[,] worldBasedHeightMap, in Vector3 spacing, in float scale)
    {
        int windowX = Mathf.CeilToInt(SearchRadius / (spacing.x * scale));
        int windowY = Mathf.CeilToInt(SearchRadius / (spacing.z * scale));
        List<JumpPadProperties> validJumpPads = new List<JumpPadProperties>();
        for (int x = 0; x < worldBasedHeightMap.GetLength(0); x++)
        {
            for (int y = 0; y < worldBasedHeightMap.GetLength(1); y++)
            { 
                Vector3 position = worldBasedHeightMap[x, y];
                if (position == Vector3.zero) break;

                HeightGradient hg = CheckHeightGradientAround(x, y, windowX, windowY, worldBasedHeightMap, (HeightGradient hg) =>
                {
                    return (hg.dy1 > SteepnessThreshold || hg.dx1 > SteepnessThreshold ||
                        hg.dy2 > SteepnessThreshold || hg.dx2 > SteepnessThreshold ||
                        hg.dxy1 > SteepnessThreshold || hg.dxy2 > SteepnessThreshold || hg.dxy3 > SteepnessThreshold || hg.dxy4 > SteepnessThreshold)
                        &&
                        hg.dy2 >= MinFlatness && hg.dx2 >= MinFlatness &&
                        hg.dy1 >= MinFlatness && hg.dx1 >= MinFlatness &&
                        hg.dxy2 >= MinFlatness && hg.dxy1 >= MinFlatness &&
                        hg.dxy4 >= MinFlatness && hg.dxy3 >= MinFlatness;
                });

                if (hg != null)
                {
                    float maxHeight = Mathf.Max(hg.dx1, hg.dx2, hg.dy1, hg.dy2, hg.dxy1, hg.dxy2, hg.dxy3, hg.dxy4);
                    validJumpPads.Add(new JumpPadProperties(maxHeight, position));
                }
            }
        }

        return validJumpPads;
    }

    private List<Cluster> ClusterPotentialJumpPads(in List<JumpPadProperties> positions)
    {
        List<Cluster> bins = new List<Cluster>();
        foreach (JumpPadProperties jumpPad in positions)
        {
            Cluster bin = null;
            if (bins.Count > 0)
            {
                float closestBinDistance = Mathf.Infinity;
                foreach (Cluster potentialBin in bins)
                {
                    float distance2D = Vector2.Distance(potentialBin.Center, new Vector2(jumpPad.Position.x, jumpPad.Position.z));
                    if (distance2D < ClusterRadius && distance2D < closestBinDistance)
                    {
                        bin = potentialBin;
                    }
                }
            }

            if (bin == null)
            {
                bin = new Cluster();
                bins.Add(bin);
            }

            bin.AddJumpPad(jumpPad);
        }
        return bins;
    }

    class HeightGradient
    {
        public float dx1, dx2, dy1, dy2, dxy1, dxy2, dxy3, dxy4 = -Mathf.Infinity;
    }

    private HeightGradient CheckHeightGradientAround(int x, int y, int windowX, int windowY, Vector3[,] heightMap, System.Func<HeightGradient, bool> gradientCheck)
    {
        float height = heightMap[x, y].y;
        HeightGradient hg = new HeightGradient();
        for (int offsetX = 1; offsetX < windowX; offsetX++)
        {
            bool maxXFit = x + offsetX < heightMap.GetLength(0);
            bool minXFit = x - offsetX >= 0;
            if (maxXFit)
                hg.dx1 = heightMap[x + offsetX, y].y - height;
            if (minXFit)
                hg.dx2 = heightMap[x - offsetX, y].y - height;

            for (int offsetY = 1; offsetY < windowY; offsetY++)
            {
                bool maxYFit = y + offsetY < heightMap.GetLength(1);
                bool minYFit = y - offsetY >= 0;
                if (maxYFit)
                    hg.dy1 = heightMap[x, y + offsetY].y - height;
                if (minYFit)
                    hg.dy2 = heightMap[x, y - offsetY].y - height;
                if (maxXFit && maxYFit)
                    hg.dxy1 = heightMap[x + offsetX, y + offsetY].y - height;
                if (minXFit && minYFit)
                    hg.dxy2 = heightMap[x - offsetX, y - offsetY].y - height;
                if (minXFit && maxYFit)
                    hg.dxy3 = heightMap[x - offsetX, y + offsetY].y - height;
                if (minYFit && maxXFit)
                    hg.dxy4 = heightMap[x + offsetX, y - offsetY].y - height;
                if (gradientCheck(hg))
                    return hg;
            }
        }

        return null;
    }

    private List<JumpPadProperties> SampleJumpPads(List<Cluster> bins)
    {
        int nbJumpPads = rng.Next(MinNbJumpPads, MaxNbJumpPads + 1);
        List<JumpPadProperties> jumpPads = new List<JumpPadProperties>();
        List<float> weights = new List<float>();
        bins.ForEach((Cluster cluster) => weights.Add(cluster.JumpPads.Count * cluster.JumpPads.Count + cluster.AverageHeight * cluster.AverageHeight));
        for (int i = 0; i < nbJumpPads && bins.Count > 0; i++)
        {
            int binIndex = rng.GetRandomWeightedIndex(weights.ToArray());
            Cluster bin = bins[binIndex];
            int jumpPadIndex = rng.Next(bin.JumpPads.Count);
            jumpPads.Add(bin.JumpPads[jumpPadIndex]);
            bins.RemoveAt(binIndex);
            weights.RemoveAt(binIndex);
        }

        return jumpPads;
    }

    private void CarveGrid(in GameObject jumpPad, NoiseGrid grid)
    {
        Renderer[] renderers = jumpPad.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        for (int i = 0; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        Vector3 minBoundFloat = grid.WorldToGrid(bounds.min);
        Vector3Int minBoundGrid = new Vector3Int(Mathf.RoundToInt(minBoundFloat.x), Mathf.FloorToInt(minBoundFloat.y), Mathf.RoundToInt(minBoundFloat.z));
        Vector3 maxBoundFloat = grid.WorldToGrid(bounds.max);
        Vector3Int maxBoundGrid = new Vector3Int(Mathf.RoundToInt(maxBoundFloat.x), Mathf.FloorToInt(maxBoundFloat.y), Mathf.RoundToInt(maxBoundFloat.z));
        Vector3 gridPosFloat = grid.WorldToGrid(jumpPad.transform.position);
        Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(gridPosFloat.x), Mathf.FloorToInt(gridPosFloat.y), Mathf.RoundToInt(gridPosFloat.z));
        float mu =  (jumpPad.transform.position.y - grid.GridToWorld(gridPos).y) / (grid.Spacing.y * grid.Scale);
        float gridValueUnder = grid[gridPos.x, gridPos.y, gridPos.z].Value;
        float gridValueAbove = grid[gridPos.x, gridPos.y + 1, gridPos.z].Value;
        for (int x = Mathf.Max(minBoundGrid.x, 1); x < Mathf.Min(maxBoundGrid.x + 1, grid.GetLength(0) - 1) ; x++)
        {
            for (int z = Mathf.Max(minBoundGrid.z, 1); z < Mathf.Min(maxBoundGrid.z + 1, grid.GetLength(2) - 1); z++)
            {
                if (gridValueUnder < 0)
                {
                    for (int y = minBoundGrid.y - 1; y >= 0; y--)
                    {
                        if (grid[x, y, z].Value > 0) break;
                        grid[x, y, z].Value = 0.00001f;
                    }
                    grid[x, Mathf.Min(minBoundGrid.y, grid.GetLength(1)), z].Value = 0.00001f;
                    grid[x, Mathf.Min(minBoundGrid.y + 1, grid.GetLength(1)), z].Value = (0.00001f * (mu - 1) + grid.Isovalue) / mu;
                }
                else
                {
                    grid[x, Mathf.Min(minBoundGrid.y, grid.GetLength(1)), z].Value = gridValueUnder;
                    grid[x, Mathf.Min(minBoundGrid.y + 1, grid.GetLength(1)), z].Value = gridValueAbove;
                }



                for (int y = minBoundGrid.y + 2; y < grid.GetLength(1); y++)
                {
                    grid[x, y, z].Value = -40;
                }
# if UNITY_EDITOR
                if (DebugDisplay)
                {
                    for (int y = minBoundGrid.y - 1; y < minBoundGrid.y + 4; y++)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.name = $"({x}, {y}, {z})_{grid[x, y, z].Value}";
                        sphere.transform.localScale = Vector3.one * 0.05f;
                        sphere.transform.position = grid.GridToWorld(new Vector3(x, y, z));
                        sphere.transform.parent = jumpPad.transform;
                    }
                }
# endif
            }
        }
    }
}
