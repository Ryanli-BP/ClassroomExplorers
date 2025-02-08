using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Health { get; protected set; }
    public Status Status { get; protected set; }
    public abstract void LoseHealth(int amount);
    public abstract void Dies();

    public abstract void TeleportTo(Vector3 position, Tile destinationTile);
}
