using UnityEngine;

[System.Serializable]
public class Floor
{
    public int floorNumber { get; private set; }
    public bool hasStairs { get; set; }

    [SerializeField]
    public Room[,] rooms;

    public buildingBounds bounds { get; set; }

    public Floor(int floorNumber, Room[,] rooms, buildingBounds bounds)
    {
        this.floorNumber = floorNumber;
        this.rooms = rooms;
        this.bounds = bounds;
        this.hasStairs = false;
    }
}
