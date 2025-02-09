using UnityEngine;

public class Boss : Entity
{
    public BossMovement Movement { get; private set; }
    public const int MAX_HEALTH = 100;

    private void Awake()
    {
        Movement = GetComponent<BossMovement>();
    }

    private void Start()
    {
        Health = MAX_HEALTH;
        Status = Status.Alive;
        UIManager.Instance.UpdateBossHealth(Health);
    }

    public override void LoseHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log($"Boss now has {Health} health.");
        UIManager.Instance.UpdateBossHealth(Health);
    }

    public override void Dies()
    {
        Status = Status.Dead;
        Debug.Log("Boss has been defeated!");
        GameManager.Instance.WinGameConditionAchieved();
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