using System;
using UnityEngine;

public enum Status { Alive, Dead }
public class Player : MonoBehaviour
{
    [SerializeField] private int playerID;

    public const int REVIVAL_COUNT = 3;

    public int Points { get; set; }
    public int Level { get; set; }
    public int Health { get; set; }
    public Status Status { get; set; }

    public int ReviveCounter { get; set; } = 0;
    void Start()
    {
        Points = 0;
        Level = 1;
        Health = 5;
        Status = Status.Alive;
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level, Health);
    }

    public int getPlayerID()
    {
        return playerID;
    }

    public void AddPoints(int amount)
    {
        Points += amount;
        Debug.Log($"Player {playerID} now has {Points} points.");
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level, Health);
    }

    public void LevelUp()
    {
        Level += 1;
        Debug.Log($"Player {playerID} leveled up to level {Level}.");
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level, Health);
    }

    public void LoseHealth(int amount)
    {
        Health = Math.Max(0,Health - amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level, Health);
    }

    public void Dies()
    {
        Status = Status.Dead;
        PlayerManager.Instance.DeadPlayers.Add(this);
        Debug.Log($"Player {playerID} has died.");
    }

    public void IncrementReviveCounter()
    {
        ReviveCounter++;
    }

    public void Revives()
    {
        ReviveCounter = 0;
        Status = Status.Alive;
        Health = 5;
        PlayerManager.Instance.DeadPlayers.Remove(this);
        Debug.Log($"Player {playerID} has revived.");
    }
}