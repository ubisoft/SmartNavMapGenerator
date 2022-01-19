using UnityEngine;
using System.Linq;

public class NoiseGrid
{
    private GridPoint[,,] _grid;
    private Vector3 _volume;
    private Vector3 _spacing;
    private float _scale;
    private float _isovalue;

    public Vector3 Volume { get => _volume; }
    public Vector3 Spacing { get => _spacing; }
    public float Scale { get => _scale; }
    public float Isovalue { get => _isovalue; }

    private NoiseGrid()
    {

    }

    public NoiseGrid(Vector3 volume, Vector3 spacing, float scale, float isovalue)
    {
        _volume = volume;
        _spacing = spacing;
        _scale = scale;
        _isovalue = isovalue;
        _grid = new GridPoint[Mathf.FloorToInt(volume.x / spacing.x), Mathf.FloorToInt(volume.y / spacing.y), Mathf.FloorToInt(volume.z / spacing.z)];
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                for (int z = 0; z < _grid.GetLength(2); z++)
                {
                    Vector3 worldspaceCoord = Vector3.Scale(new Vector3(x, y, z), Spacing);
                    _grid[x, y, z] = new GridPoint(worldspaceCoord);
                }
            }
        }
    }

    public NoiseGrid(NoiseGrid grid)
    {
        _volume = grid._volume;
        _spacing = grid._spacing;
        _scale = grid._scale;
        _isovalue = grid._isovalue;
        _grid = new GridPoint[Mathf.FloorToInt(grid._volume.x / grid._spacing.x), Mathf.FloorToInt(grid._volume.y / grid._spacing.y), Mathf.FloorToInt(grid._volume.z / grid._spacing.z)];
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                for (int z = 0; z < _grid.GetLength(2); z++)
                {
                    _grid[x, y, z] = new GridPoint(grid[x,y,z]);
                }
            }
        }
    }
    
    public GridPoint this[Vector3Int gridPos]
    {
        get => _grid[gridPos.x, gridPos.y, gridPos.z];
        set => _grid[gridPos.x, gridPos.y, gridPos.z] = value;
    }

    public GridPoint this[int x, int y, int z]
    {
        get => _grid[x, y, z];
        set => _grid[x, y, z] = value;
    }

    public void SetValueAt(Vector3Int gridPos, float value)
    {
        this[gridPos].Value = value;
    }

    public Vector3 GridToWorld(in Vector3 gridPos)
    {
        Vector3 worldPos = gridPos;
        worldPos.Scale(Spacing);
        return worldPos * Scale;
    }

    public Vector3 WorldToGrid(in Vector3 worldPos)
    {
        return new Vector3(worldPos.x / (Spacing.x * Scale),
                                      worldPos.y / (Spacing.y * Scale),
                                      worldPos.z / (Spacing.z * Scale));
    }

    public enum RoundingType
    {
        Floor,
        Nearest,
        Ceil,
    }

    public Vector3Int WorldToGrid(in Vector3 worldPos, RoundingType roundingType)
    {
        Vector3 gridPos = WorldToGrid(worldPos);
        if (roundingType == RoundingType.Floor)
            return Vector3Int.FloorToInt(gridPos);
        else if (roundingType == RoundingType.Ceil)
            return Vector3Int.CeilToInt(gridPos);
        else
            return Vector3Int.RoundToInt(gridPos);
    }

    public NoiseGrid SubGrid(int start1, int start2, int start3, int end1, int end2, int end3)
    {
        end1 = end1 < 0 ? _grid.GetLength(0) + 1 + end1 : end1;
        end2 = end2 < 0 ? _grid.GetLength(1) + 1 + end2 : end2;
        end3 = end3 < 0 ? _grid.GetLength(2) + 1 + end3 : end3; 
        
        NoiseGrid subGrid = new NoiseGrid();
        subGrid._scale = _scale;
        Vector3 volume = new Vector3(end1 - start1, end2 + 1 - start2, end3 - start3);
        volume.Scale(Spacing);
        subGrid._volume = volume;
        subGrid._spacing = _spacing;
        subGrid._isovalue = _isovalue;

        subGrid._grid = new GridPoint[end1 - start1, end2 - start2, end3 - start3];
        Vector3 startPosition = _grid[start1, start2, start3].Position;
        ForEach(start1, start2, start3, end1, end2, end3, (gridPoint, gridPos) =>
        {
            Vector3Int subGridPos = gridPos - new Vector3Int(start1, start2, start3);
            subGrid[subGridPos] = new GridPoint(gridPoint);
            subGrid[subGridPos].Position -= startPosition;
        });
        return subGrid;
    }

    public GridPoint MaxValuePoint()
    {
        float maxValue = -Mathf.Infinity;
        GridPoint maxPoint = null;
        ForEach(gridPoint =>
        {
            if (gridPoint.Value > maxValue)
            {
                maxValue = gridPoint.Value;
                maxPoint = gridPoint;
            }
        });
        return maxPoint;
    }

    public GridPoint MinValuePoint()
    {
        float minValue = Mathf.Infinity;
        GridPoint minPoint = null;
        ForEach(gridPoint =>
        {
            if (gridPoint.Value < minValue)
            {
                minValue = gridPoint.Value;
                minPoint = gridPoint;
            }
        });
        return minPoint;
    }

    public void ForEach(System.Action<GridPoint> action)
    {
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                for (int z = 0; z < _grid.GetLength(2); z++)
                {
                    action.Invoke(_grid[x, y, z]);
                }
            }
        }
    }

    public void ForEach(System.Action<GridPoint, Vector3Int> action)
    {
        ForEach(0, 0, 0, -1, -1, -1, action);
    }

    public void ForEach(int start1, int start2, int start3, int end1, int end2, int end3, System.Action<GridPoint> action)
    {
        ForEach(start1, start2, start3, end1, end2, end3, gridPoint =>
        {
            action.Invoke(gridPoint);
        });
    }

    public void ForEach(int start1, int start2, int start3, int end1, int end2, int end3, System.Action<GridPoint, Vector3Int> action)
    {
        end1 = end1 < 0 ? _grid.GetLength(0) + 1 + end1 : end1;
        end2 = end2 < 0 ? _grid.GetLength(1) + 1 + end2 : end2;
        end3 = end3 < 0 ? _grid.GetLength(2) + 1 + end3 : end3;
        for (int x = start1; x < end1; x++)
        {
            for (int y = start2; y < end2; y++)
            {
                for (int z = start3; z < end3; z++)
                {
                    action.Invoke(_grid[x, y, z], new Vector3Int(x, y, z));
                }
            }
        }
    }

    public int GetLength(int dimension)
    {
        return _grid.GetLength(dimension);
    }

    public NoiseGrid Clone()
    {
        return new NoiseGrid(this);
    }

    private static NoiseGrid NullGrid(Vector3 volume, Vector3 spacing, float scale, float isovalue)
    {
        NoiseGrid grid = new NoiseGrid();
        grid._volume = volume;
        grid._spacing = spacing;
        grid._scale = scale;
        grid._isovalue = isovalue;
        grid._grid = new GridPoint[Mathf.FloorToInt(volume.x / spacing.x), Mathf.FloorToInt(volume.y / spacing.y), Mathf.FloorToInt(volume.z / spacing.z)];
        return grid;
    }

    public static NoiseGrid EvenPaddedGrid(NoiseGrid grid, int evenPadding)
    {
        if (evenPadding % 2 != 0) throw new System.ArgumentException("Padding has to be even.");
        NoiseGrid paddedGrid = NullGrid(grid.Volume + grid.Spacing * evenPadding, grid.Spacing, grid.Scale, grid.Isovalue);

        paddedGrid.ForEach((gridPoint, gridPos) =>
        {
            paddedGrid[gridPos] = new GridPoint(Vector3.Scale(gridPos, paddedGrid._spacing));
            Vector3Int nonPaddedPos = gridPos - Vector3Int.one * (evenPadding / 2);
            if (nonPaddedPos.x >= 0 && nonPaddedPos.x < grid.GetLength(0) &&
                nonPaddedPos.y >= 0 && nonPaddedPos.y < grid.GetLength(1) &&
                nonPaddedPos.z >= 0 && nonPaddedPos.z < grid.GetLength(2))
            {
                paddedGrid[gridPos].Value = grid[nonPaddedPos].Value;
            }
            else
                paddedGrid[gridPos].Value = -40;
        });
        return paddedGrid;
    }
}
