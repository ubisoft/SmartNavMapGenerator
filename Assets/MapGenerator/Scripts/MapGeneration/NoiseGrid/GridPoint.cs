using UnityEngine;

public class GridPoint
{
    private Vector3 _chunk = Vector3.zero;
    private float _value = 0f;
    private Vector3 _position = Vector3.zero;
    public GridPoint()
    {
    }

    public GridPoint(Vector3 position)
    {
        _position = position;
    }

    public GridPoint(GridPoint gridPoint)
    {
        _chunk = gridPoint._chunk;
        _value = gridPoint._value;
        _position = gridPoint._position;
    }

    public Vector3 Chunk
    {
        get
        {
            return _chunk;
        }
        set
        {
            _chunk = new Vector3(value.x, value.y, value.z);
        }
    }
    public Vector3 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = new Vector3(value.x, value.y, value.z);
        }
    }
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
        }
    }
    public override string ToString()
    {
        return string.Format("chunk[{0},{1},{2}] position[{3},{4},{5}] value={6}", Chunk.x, Chunk.y, Chunk.z, Position.x, Position.y, Position.z, Value);
    }
}
