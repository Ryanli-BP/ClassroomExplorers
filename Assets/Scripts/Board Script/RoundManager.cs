using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[DefaultExecutionOrder(-10)]
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
        UIManager.Instance.UpdateCurrentPlayerTurn(Turn);
        GameInitializer.Instance.ConfirmManagerReady("RoundManager");
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
        UIManager.Instance.UpdateCurrentPlayerTurn(Turn);
    }

    public void IncrementRound()
    {
        round++;
        Debug.Log($"Round {round}");
        UIManager.Instance.UpdateRound(round);

        if(PlayerManager.Instance.DeadPlayers.Count > 0)
        {
            IncrementReviveCounter();
        }
    }

    public void IncrementReviveCounter()
    {
        var deadPlayersCopy = new List<Player>(PlayerManager.Instance.DeadPlayers);

        foreach (var player in deadPlayersCopy)
        {
            player.IncrementReviveCounter();
            if (player.ReviveCounter == Player.REVIVAL_COUNT)
            {
                player.Revives();
            }
        }
    }

    public IEnumerator GiveRoundPoints()
    {
        if(round == 1)
        {
            yield break;
        }
        
        int roundPointsIndex = PlayerManager.Instance.CurrentHighLevel - 1;

        foreach (var player in PlayerManager.Instance.GetPlayerList())
        {
            yield return StartCoroutine(player.AddPoints(roundPoints[roundPointsIndex]));

        }
    }

    public void ApplyRoundBuff()
    {
        foreach (var player in PlayerManager.Instance.GetPlayerList())
        {
            player.UpdateBuffDurations(); //automatically disable buffs when duration over
        }
        if (GameConfigManager.Instance.GetCurrentRules().haveBoss)
        {
            BossManager.Instance.activeBoss.Buffs.UpdateBuffDurations();
        }


    }
}
