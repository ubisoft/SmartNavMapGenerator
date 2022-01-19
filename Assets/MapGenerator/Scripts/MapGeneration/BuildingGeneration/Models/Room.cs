using UnityEngine;

public class Room
{
    public Wall[] walls { get; set; } = new Wall[4];

    private Vector3 position;

    public bool hasRoof { get; set; }
    public bool hasFloor{ get; set; }
    public bool hasStair { get; set; }

    public RoomRay roomRay { get; set; }

    public int floorNumber { get; set; }

    public Room(Vector3 position, int floorNumber, bool hasRoof = false, bool hasFloor = true, RoomRay roomRay=null)
    {
        this.position = position;
        this.floorNumber = floorNumber;
        this.hasRoof = hasRoof;
        this.hasFloor = hasFloor;
        this.hasStair= hasStair;
        this.roomRay = roomRay;
    }

    public Vector3 RoomPosition
    {
        get
        {
            return this.position;
        }
    }

}
