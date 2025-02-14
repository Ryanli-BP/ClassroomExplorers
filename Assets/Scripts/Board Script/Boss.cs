using UnityEngine;

public class Boss : Entity
{
    public BossMovement Movement { get; private set; }
    public const int MAX_HEALTH = 100;
    [SerializeField] private BossBuffs bossBuffs = new BossBuffs();
    public BossBuffs BossBuffs => bossBuffs;


    private void Awake()
    {
        Movement = GetComponent<BossMovement>();        
    }

    private void Start()
    {
        Health = MAX_HEALTH;
        Status = Status.Alive;
        UIManager.Instance.UpdateBossHealth(Health);

        /*Buffs.AddBuff(BuffType.AttackUp, 2, 2); //for test
        Buffs.AddBuff(BuffType.DefenseUp, 1, 2); //for test
        Buffs.AddBuff(BuffType.ExtraDice, 2, 2); //for testing*/
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

    public override void AddBuff(BuffType type, int value, int duration)
    {
        bossBuffs.AddBuff(type, value, duration);
    }

    public override void UpdateBuffDurations()
    {
        bossBuffs.UpdateBuffDurations();
    }

    public override void TeleportTo(Vector3 position, Tile destinationTile)
    {
        // Adjust Y position for proper height above tile
        Vector3 teleportPosition = new Vector3(position.x, position.y + 0.7f * ARBoardPlacement.worldScale, position.z);
        
        // Update player position
        transform.position = teleportPosition;
        
        // Update current tile reference
        Movement.CurrentTile = destinationTile;
    
        
        Debug.Log($"Boss teleported to position {position}");
    }
}

[System.Serializable]
public class BossBuffs : EntityBuffs
{
    public override void AddBuff(BuffType type, int value, int duration)
    {
        if (type == BuffType.DoublePoints)
        {
            Debug.LogWarning("Cannot add player-specific buffs to a boss");
            return;
        }
        base.AddBuff(type, value, duration);
    }
}