using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMap
{
    public static Vector3[,] WorldBasedHeightMap(in NoiseGrid grid, in float offset = -1, in List<GameObject> buildingGOs = null)
    {
        bool considerBuildings = buildingGOs != null;
        List<Bounds> buildingBounds = null;
        if (considerBuildings)
        {
            buildingBounds = new List<Bounds>();
            foreach (GameObject building in buildingGOs)
            {
                Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
                Bounds combinedBounds = renderers[0].bounds;

                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }
                buildingBounds.Add(combinedBounds);
            }
        }

        Vector3[,] heights = new Vector3[grid.GetLength(0) - 1, grid.GetLength(2) - 1];
        int yGridPos = 1;
        if (offset > 0)
            yGridPos = Mathf.CeilToInt(offset / grid.Spacing.y);
        for (int x = 1; x < heights.GetLength(0); x++)
        {
            for (int z = 1; z < heights.GetLength(1); z++)
            {
                if (offset > 0)
                {
                    Vector3 position = grid[x, yGridPos, z].Position;
                    position.y = offset;
                    position *= grid.Scale;
                    heights[x - 1, z - 1] = position;
                }
                else
                {
                    heights[x - 1, z - 1] = grid[x, yGridPos - 1, z].Position * grid.Scale;
                }
                for (int y = yGridPos; y < grid.GetLength(1); y++)
                {
                    // If true, we went from inside the terrain to outside, so it is a floor.
                    GridPoint p1 = grid[x, y, z];
                    GridPoint p2 = grid[x, y - 1, z];
                    if (p1.Value < 0 && p2.Value > 0)
                    {
                        Vector3 position = p1.Position;
                        float mu = (grid.Isovalue - p1.Value) / (p2.Value - p1.Value);
                        position.y = p1.Position.y + mu * (p2.Position.y - p1.Position.y);
                        position *= grid.Scale;
                        heights[x - 1, z - 1] = position;
                        if (considerBuildings)
                        {
                            foreach (Bounds bounds in buildingBounds)
                            {
                                if (bounds.Contains(new Vector3(position.x, bounds.center.y, position.z)))
                                {
                                    heights[x - 1, z - 1].y = bounds.max.y;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return heights;
    }
}
