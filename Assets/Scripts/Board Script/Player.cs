using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int playerID;

    public int Points { get; set; }
    public int Level { get; set; }

    void Start()
    {
        Points = 0;
        Level = 1;
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level);
    }

    public int getPlayerID()
    {
        return playerID;
    }

    public void AddPoints(int amount)
    {
        Points += amount;
        Debug.Log($"Player {playerID} now has {Points} points.");
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level);
    }

    public void LevelUp()
    {
        Level += 1;
        Debug.Log($"Player {playerID} leveled up to level {Level}.");
        UIManager.Instance.UpdatePlayerStats(playerID, Points, Level);
    }
}