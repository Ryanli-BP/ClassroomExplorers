using UnityEngine;
using System.Collections.Generic;
using UnityEditor.VisionOS;
using System;

public class RoundManager : MonoBehaviour
{

    public static RoundManager Instance;
    private int round;
    public int Turn { get; set; } = 1;
    [SerializeField] private List<int> roundPoints = new List<int> { 1, 3, 7 };

    void Start()
    {
        round = 0;
        UIManager.Instance.UpdateRound(round);
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void IncrementTurn()
    {
        Turn = (Turn % PlayerManager.Instance.GetNumOfPlayers()) + 1;
        Debug.Log($"Turn {Turn}");
    }

    public void IncrementRound()
    {
        round++;
        Debug.Log($"Round {round}");
        UIManager.Instance.UpdateRound(round);
    }

    public void GiveRoundPoints()
    {
        int roundPointsIndex = PlayerManager.Instance.CurrentHighLevel - 1;

        foreach (var player in PlayerManager.Instance.GetPlayerList())
        {
            player.AddPoints(roundPoints[roundPointsIndex]);
        }
    }
}
