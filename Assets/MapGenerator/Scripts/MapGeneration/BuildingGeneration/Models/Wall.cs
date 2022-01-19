using UnityEngine;

public class Wall
{
    public enum WallType
    {
        Normal, 
        PotentialStair,
        Blank
    }

    public WallType wallTypeSelected { get; set; } = WallType.Normal;

    public Vector3 position { get; private set; }

    public Quaternion rotation { get; private set; }

    public Wall(Vector3 position, Quaternion rotation, WallType wallType = WallType.Normal)
    {
        this.wallTypeSelected = wallType;
        this.position = position;
        this.rotation = rotation;
    }
}
