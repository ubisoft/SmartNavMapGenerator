using UnityEngine;

public class RoomRay
{
    [SerializeField]
    public float rayDistance { get; private set; }

    public Vector3 rayPosition { get; private set; }

    public RoomRay(Vector3 rayPosition, float distance)
    {
        this.rayPosition = rayPosition;
        this.rayDistance = rayDistance;
    }
}
