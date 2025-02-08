using UnityEngine;

public class Boss : Entity
{
    public BossMovement Movement { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<BossMovement>();
    }

    private void Start()
    {
        Health = 100;
        Status = Status.Alive;
    }

    public override void LoseHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log($"Boss now has {Health} health.");
        // Add UI update for boss health if needed
    }

    public override void Dies()
    {
        Status = Status.Dead;
        Debug.Log("Boss has been defeated!");
        // Add any boss death logic
    }

    public override void TeleportTo(Vector3 position, Tile destinationTile)
    {
        // Adjust Y position for proper height above tile
        Vector3 teleportPosition = new Vector3(position.x, position.y + 0.2f * ARBoardPlacement.worldScale, position.z);
        
        // Update player position
        transform.position = teleportPosition;
        
        // Update current tile reference
        Movement.CurrentTile = destinationTile;
    
        
        Debug.Log($"Boss teleported to position {position}");
    }
}