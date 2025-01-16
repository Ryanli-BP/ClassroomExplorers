using System;
using UnityEngine;

public enum Status { Alive, Dead }
public class Player : MonoBehaviour
{
    [SerializeField] private int playerID;

    public const int REVIVAL_COUNT = 3;
    public const int MAX_HEALTH = 10;

    public int Points { get; set; }
    public int Level { get; set; }
    public int Health { get; set; }
    public Status Status { get; set; }

    public int ReviveCounter { get; set; } = 0;
    void Start()
    {
        Points = 0;
        Level = 1;
        Health = MAX_HEALTH;
        Status = Status.Alive;
        
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
        UIManager.Instance.UpdatePlayerLevel(playerID, Level);
        UIManager.Instance.UpdatePlayerPoints(playerID, Points);
    }

    public int getPlayerID()
    {
        return playerID;
    }

    public void AddPoints(int amount)
    {
        Points += amount;
        Debug.Log($"Player {playerID} now has {Points} points.");
        UIManager.Instance.UpdatePlayerPoints(playerID, Points);
    }

    public void LevelUp()
    {
        Level += 1;
        Debug.Log($"Player {playerID} leveled up to level {Level}.");
        UIManager.Instance.UpdatePlayerLevel(playerID, Level);
    }

    public void LoseHealth(int amount)
    {
        Health = Math.Max(0,Health - amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }

    public void Dies()
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
    }

    public void HealPLayer(int amount)
    {
        Health = Math.Min(MAX_HEALTH, Health + amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }
}