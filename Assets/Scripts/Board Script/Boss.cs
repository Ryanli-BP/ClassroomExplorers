using System.Collections;
using UnityEngine;

public class Boss : Entity
{
    public BossMovement Movement { get; private set; }
    public int MAX_HEALTH { get; private set; } = 100;
    [SerializeField] private BossBuffs bossBuffs = new BossBuffs();
    public BossBuffs BossBuffs => bossBuffs;
    public const float AboveTileOffset = 0.5f; // Offset to place boss above the tile

    public static Boss Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Movement = GetComponent<BossMovement>();        
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameConfigManager.Instance.IsFetchComplete);

        MAX_HEALTH = 20 * GameConfigManager.Instance.numOfPlayers - 20;
        Health = MAX_HEALTH;
        Status = Status.Alive;

        /*Buffs.AddBuff(BuffType.AttackUp, 2, 2); //for test
        Buffs.AddBuff(BuffType.DefenseUp, 1, 2); //for test*/
        BossBuffs.AddBuff(BuffType.ExtraDice, 1, 99); //for testing
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
        Vector3 teleportPosition = new Vector3(position.x, position.y + (AboveTileOffset * BoardGenerator.BoardScale * ARBoardPlacement.worldScale), position.z);
        
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