using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Resolution")]
    public Vector3 Volume = new Vector3(20, 10, 20);
    public Vector3 Spacing = new Vector3(0.15f, 0.15f, 0.15f);
    public float Scale = 6;
    public float surfacelevel = 0;

    // Generation settings
    [Header("General")]
    public int seed = 0;
    [Range(0, 25)]
    public float PrewarpStrength = 0;
    public float Height = 2; 
    public float HardFloor = 1.5f;
    public float HardFloorWeight = 40;
    public List<Octave> Octaves = new List<Octave> { new Octave(1.01f, 1f), new Octave(0.5f, 2f), new Octave(0.2f, 6f)};

    [Header("Overhangs")]
    public bool GenerateOverhangs = true;
    [Range(0f, 1f)]
    public float OverhangHeight = 0.5f;
    [Range(0f, 1f)]
    public float OverhangHeightWarping = 0.5f;
    public float OverhangStrength = 20;

    [Header("Terraces")]
    public bool GenerateTerraces = false;
    [Range(0f, 1f)]
    public float TerraceHeight = 0.05f;
    [Range(0f, 1f)]
    public float TerraceHeightWarping = 0.5f;
    public float TerraceStrength = 20;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private GridPoint[,,] grid = null;

    public ref GridPoint[,,] Grid
    {
        get
        {
            return ref grid;
        }
    }

    private List<PlatformGenerationStep> generationSteps = new List<PlatformGenerationStep>();

    public void AddGenerationStep(PlatformGenerationStep step)
    {
        generationSteps.Add(step);
    }

    public NoiseGrid Generate()
    {
        Random.InitState(seed);
        Perlin.seed = seed;
        foreach (ITerrainGenerationStep step in generationSteps)
        {
            step.Prepare(this);
        }
        return TerrainNoiseGrid();
    }

    public Mesh ComputeMesh(NoiseGrid grid)
    {
        MarchingCube marchingCube = new MarchingCube();
        Mesh mesh = marchingCube.ComputeMesh(grid, surfacelevel, Volume);
        SetUVs(ref mesh);
        return mesh;
    }

    private NoiseGrid TerrainNoiseGrid()
    {
        NoiseGrid grid = new NoiseGrid(Volume, Spacing, Scale, surfacelevel);
        grid.ForEach((gridPoint, gridPos) =>
        {
            gridPoint.Chunk = this.transform.position;
            Vector3 worldspaceCoord = gridPoint.Position;

            float uulf = Perlin.Noise(worldspaceCoord * 0.000718f) * 2;
            float uulf2 = Perlin.Noise(worldspaceCoord * 0.000632f);
            float uulf3 = Perlin.Noise(worldspaceCoord * 0.000695f);

            Vector3 ulf_rand = Vector3.zero;
            ulf_rand.x = Perlin.Noise(worldspaceCoord * 0.0041f * 0.971f) * 0.64f
                        + Perlin.Noise(worldspaceCoord * 0.0041f * 0.461f) * 0.32f;
            ulf_rand.y = Perlin.Noise(worldspaceCoord * 0.0041f * 0.997f) * 0.64f
                        + Perlin.Noise(worldspaceCoord * 0.0041f * 0.453f) * 0.32f;
            ulf_rand.z = Perlin.Noise(worldspaceCoord * 0.0041f * 1.032f) * 0.64f
                        + Perlin.Noise(worldspaceCoord * 0.0041f * 0.511f) * 0.32f;
            Vector3 prewarpWS = worldspaceCoord + ulf_rand * PrewarpStrength * MathUtils.Saturate(uulf3 * 1.4f - 0.3f);

            gridPoint.Value = -prewarpWS.y + Height;
            foreach (Octave octave in Octaves)
            {
                gridPoint.Value += Perlin.Noise(prewarpWS * octave.Frequency) * octave.Amplitude;
            }

            // Hard floor
            if (worldspaceCoord.y < HardFloor)
            {
                gridPoint.Value += HardFloorWeight;
            }

            //float terraces_can_warp = 0.5f * uulf2;
            //float terrace_freq_y = 0.05f;
            //float terrace_str = 5 * Saturate(uulf * 2);  // careful - high str here diminishes strength of noise, etc.
            //float fy = -Mathf.Lerp(worldspaceCoord.y, prewarpWS.y, terraces_can_warp) * terrace_freq_y;
            //float orig_t = fy - Mathf.Floor(fy);
            //float t = orig_t;
            //t = smooth_snap(t, 16);  // faster than using 't = t*t*(3-2*t)' four times
            //fy = Mathf.Floor(fy) + t;
            ////density += fy * terrace_str;
            //density += (t - orig_t) * overhang_str;

            if (GenerateTerraces)
            {
                float terrace_can_warp = TerraceHeightWarping * uulf2;
                float terrace_str = TerraceStrength * MathUtils.Saturate(uulf * 2);  // careful - too much here and LODs interfere (most visible @ silhouettes because zbias can't fix those).
                float fy = -Mathf.Lerp(worldspaceCoord.y, prewarpWS.y, terrace_can_warp) * TerraceHeight;
                gridPoint.Value += fy * terrace_str;
            }

            if (GenerateOverhangs)
            {
                float overhang_can_warp = OverhangHeightWarping * uulf2;
                float overhang_str = OverhangStrength * MathUtils.Saturate(uulf * 2);  // careful - too much here and LODs interfere (most visible @ silhouettes because zbias can't fix those).
                float fy = -Mathf.Lerp(worldspaceCoord.y, prewarpWS.y, overhang_can_warp) * OverhangHeight;
                float orig_t = fy - Mathf.Floor(fy);
                float t = orig_t;
                t = MathUtils.smooth_snap(t, 16);  // faster than using 't = t*t*(3-2*t)' four times
                gridPoint.Value += (t - orig_t) * overhang_str;
            }

            foreach (ITerrainGenerationStep step in generationSteps)
            {
                step.Execute(prewarpWS, ref gridPoint);
            }

            // Trying to close map edges
            //Vector3 edgeOffset = Abs(worldspaceCoord*2) - Volume + (spacing / 2) * Vector3.one;
            //float edgeWeight = Saturate(Mathf.Sign(Mathf.Max(Mathf.Max(edgeOffset.x, edgeOffset.y), edgeOffset.z)));
            //density = density * (1 - edgeWeight) - 100 * edgeWeight;
            if (gridPos.x == 0 | gridPos.x == grid.GetLength(0) - 1 | gridPos.z == 0 | gridPos.z == grid.GetLength(2) - 1)
            {
                gridPoint.Value = -40;
            }
        });
        return grid;
    }

    private void SetUVs(ref Mesh mesh)
    {
        float maxYBound = mesh.bounds.max.y;
        Vector3[] vertices = mesh.vertices.Clone() as Vector3[];
        Vector2[] uv2 = new Vector2[vertices.Length];
        for (int i = 0; i < uv2.Length; ++i)
        {
            uv2[i] = new Vector2(Mathf.SmoothStep(0, 1, vertices[i].y / maxYBound), 0);
        }
        mesh.uv2 = uv2;
    }
}
