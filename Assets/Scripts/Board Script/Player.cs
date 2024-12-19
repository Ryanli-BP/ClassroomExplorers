using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;
    private int points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize player points
        points = 0;
    }

    // Method to add points to the player
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