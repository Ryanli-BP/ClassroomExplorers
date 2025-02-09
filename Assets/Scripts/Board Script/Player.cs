using System;
using UnityEngine;
using System.Collections;

public enum Status { Alive, Dead }
public class Player : Entity
{
    [SerializeField] private int playerID;

    public const int REVIVAL_COUNT = 3;
    public const int MAX_HEALTH = 10;
    public const int MAX_LEVEL = 10;

    public int Points { get; set; }
    public int Level { get; set; } = 1;

    public int ReviveCounter { get; set; } = 0;
    void Start()
    {
        if (playerID <= PlayerManager.Instance.numOfPlayers)
        {
            Points = 0;
            Level = 1;
            Health = MAX_HEALTH;
            Status = Status.Alive;

            StartCoroutine(InitializePlayerUI());
        }
    }

    private IEnumerator InitializePlayerUI()
    {
        Debug.Log($"{playerID}");
        while (UIManager.Instance == null || !GameInitializer.Instance.IsGameInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Update UI once UIManager is ready
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
        UIManager.Instance.UpdatePlayerLevel(playerID, Level);
        UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetLevelUpPoints(Level));
    }

    public int getPlayerID()
    {
        return playerID;
    }

    public void SetPlayerID(int id)
    {
        Debug.Log($"Player {playerID} has been assigned ID {id}");
        playerID = id;
    }


    public void AddPoints(int amount)
    {
        Points = Math.Max(0, Points + amount);
        Debug.Log($"Player {playerID} now has {Points} points.");
        UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetLevelUpPoints(Level));
    }

    public void LevelUp()
    {
        if (Level >= MAX_LEVEL)
        {
            return;
        }
        
        Level += 1;
        Debug.Log($"Player {playerID} leveled up to level {Level}.");
        UIManager.Instance.UpdatePlayerLevel(playerID, Level);
        UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetLevelUpPoints(Level));
        UIManager.Instance.DisplayLevelUp();
    }

    public override void LoseHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }

    public override void Dies()
    {
        Status = Status.Dead;
        PlayerManager.Instance.DeadPlayers.Add(this);
        UIManager.Instance.UpdateReviveCounter(playerID, REVIVAL_COUNT - ReviveCounter);
        Debug.Log($"Player {playerID} has died.");
    }

    public void IncrementReviveCounter()
    {
        ReviveCounter++;
        UIManager.Instance.UpdateReviveCounter(playerID, REVIVAL_COUNT - ReviveCounter);
    }

    public void Revives()
    {
        ReviveCounter = 0;
        Status = Status.Alive;
        Health = MAX_HEALTH;
        PlayerManager.Instance.DeadPlayers.Remove(this);
        UIManager.Instance.ClearReviveCounter(playerID);
        Debug.Log($"Player {playerID} has revived.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }

    public override void TeleportTo(Vector3 position, Tile destinationTile)
    {
        // Adjust Y position for proper height above tile
        Vector3 teleportPosition = new Vector3(position.x, position.y + 0.2f * ARBoardPlacement.worldScale, position.z);
        
        // Update player position
        transform.position = teleportPosition;
        
        // Update current tile reference
        GetComponent<PlayerMovement>().CurrentTile = destinationTile;
    
        
        Debug.Log($"Player {playerID} teleported to position {position}");
    }

    public void Heal(int amount)
    {
        if (Status == Status.Dead) //cannot heal a dead player
        {
            return;
        }
        Health = Math.Min(MAX_HEALTH, Health + amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }
}