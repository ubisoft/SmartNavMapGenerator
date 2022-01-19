using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlatformGenerationStep : ITerrainGenerationStep
{
    public int MinNbPlatforms = 0;
    public int MaxNbPlatforms = 1;
    public float InnerRadius = 3;
    public float OuterRadius = 4; 
    public float MinHeight = 0;
    public float MaxHeight = 4;
    public bool NonOverlapping = true;
    public float PlatformsWarping = 1;
    public int Tries = 10000;

    protected List<Vector3> platforms = new List<Vector3>();
    public List<Vector3> Platforms
    {
        get
        {
            return platforms;
        }
    }

    public void SetEnabledFunc(System.Func<bool> enabled)
    {
        Enabled = enabled;
    }

    public void SamplePlatforms(in float hardFloor, in Vector3 bounds)
    {
        int nbrPlatforms = Random.Range(MinNbPlatforms, MaxNbPlatforms);
        platforms.Clear();
        for (int i = 0; i < nbrPlatforms; i++)
        {
            int try_idx = 0;
            Vector3 pos;
            bool overlappingPlatform = false;
            float height = hardFloor + Random.Range(MinHeight, MaxHeight);
            do
            {
                pos = new Vector3(
                        Random.Range(OuterRadius, bounds.x - OuterRadius),
                        height,
                        Random.Range(OuterRadius, bounds.z - OuterRadius)
                    );
                try_idx++;
                if (NonOverlapping)
                    overlappingPlatform = CheckForOverlap(pos);
            } while (try_idx < Tries && overlappingPlatform);

            if (!overlappingPlatform)
            {
                platforms.Add(pos);
            }
            else
            {
                Debug.LogWarning($"Unable to fit {nbrPlatforms} non-overlapping platforms in the map, settling for {platforms.Count}.");
                break;
            }
        }
    }

    bool CheckForOverlap(in Vector3 pos)
    {
        foreach (Vector3 platform in platforms)
        {
            if (Vector3.Distance(platform, pos) < (InnerRadius * 2))
                return true;
        }
        return false;
    }

    public override void Prepare(in TerrainGenerator terrainGenerator)
    {
        if (Enabled())
            SamplePlatforms(terrainGenerator.HardFloor, terrainGenerator.Volume);
    }

    public override void Execute(in Vector3 prewarpWS, ref GridPoint gridPoint)
    {
        if (!Enabled()) return;
        Vector2 xz = new Vector2(prewarpWS.x, prewarpWS.z);
        foreach (Vector3 p in platforms)
        {
            float distanceFromFlatPlatform = (xz - new Vector2(p.x, p.z)).magnitude;
            float flattenAmout = MathUtils.Saturate((OuterRadius - distanceFromFlatPlatform) / (OuterRadius - InnerRadius)) * PlatformsWarping;
            gridPoint.Value = Mathf.Lerp(gridPoint.Value, -gridPoint.Position.y + p.y, flattenAmout);
        }
    }
}
