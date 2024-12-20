using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int playerID;
    private int points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize player points
        points = 0;
    }

    public int getPlayerID()
    {
        return playerID;
    }
    public void AddPoints(int amount)
    {
        points += amount;
        Debug.Log($"Player {playerID} now has {points} points.");
    }

    // Method to reset player points
    public void ResetPoints()
    {
        points = 0;
        Debug.Log($"Player {playerID}'s points have been reset.");
    }
}