using UnityEngine;

public abstract class ITerrainGenerationStep
{
    public System.Func<bool> Enabled;

    public abstract void Prepare(in TerrainGenerator terrainGenerator);

    public abstract void Execute(in Vector3 prewarpWS, ref GridPoint gridPoint);
}
