using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    private class RewardOption
    {
        public BuffType BuffType { get; set; }
        public int Value { get; set; }
        public int Duration { get; set; }
        public int ChancePercent { get; set; }
        public string Description { get; set; }
    }

    private Dictionary<QuizReward, List<RewardOption>> rewardTiers;
    public static RewardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeRewardTiers();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeRewardTiers()
    {
        rewardTiers = new Dictionary<QuizReward, List<RewardOption>>
        {
            [QuizReward.SmallReward] = new List<RewardOption>
            {
                new RewardOption { BuffType = BuffType.AttackUp, Value = 1, Duration = 2, ChancePercent = 30, Description = "+1 Attack for 2 rounds" },
                new RewardOption { BuffType = BuffType.DefenseUp, Value = 1, Duration = 2, ChancePercent = 30, Description = "+1 Defense for 2 rounds" },
                new RewardOption { BuffType = BuffType.EvadeUp, Value = 1, Duration = 2, ChancePercent = 30, Description = "+1 Evade for 2 rounds" },
                new RewardOption { BuffType = BuffType.DoublePoints, Value = 1, Duration = 2, ChancePercent = 10, Description = "Double Points for 3 rounds" }
            },
            
            [QuizReward.MediumReward] = new List<RewardOption>
            {
                new RewardOption { BuffType = BuffType.AttackUp, Value = 2, Duration = 2, ChancePercent = 25, Description = "+2 Attack for 2 rounds" },
                new RewardOption { BuffType = BuffType.DefenseUp, Value = 2, Duration = 2, ChancePercent = 25, Description = "+2 Defense for 2 rounds" },
                new RewardOption { BuffType = BuffType.EvadeUp, Value = 2, Duration = 2, ChancePercent = 25, Description = "+2 Evade for 2 rounds" },
                new RewardOption { BuffType = BuffType.DoublePoints, Value = 1, Duration = 3, ChancePercent = 20, Description = "Double Points for 4 rounds" },
                new RewardOption { BuffType = BuffType.ExtraDice, Value = 1, Duration = 2, ChancePercent = 5, Description = "+1 Extra Dice for 2 rounds" },
            },
            
            [QuizReward.BigReward] = new List<RewardOption>
            {
                new RewardOption { BuffType = BuffType.AttackUp, Value = 3, Duration = 3, ChancePercent = 19, Description = "+3 Attack for 3 rounds" },
                new RewardOption { BuffType = BuffType.DefenseUp, Value = 3, Duration = 3, ChancePercent = 19, Description = "+3 Defense for 3 rounds" },
                new RewardOption { BuffType = BuffType.EvadeUp, Value = 3, Duration = 3, ChancePercent = 19, Description = "+3 Evade for 3 rounds" },
                new RewardOption { BuffType = BuffType.DoublePoints, Value = 1, Duration = 4, ChancePercent = 19, Description = "Double Points for 3 rounds" },
                new RewardOption { BuffType = BuffType.TriplePoints, Value = 1, Duration = 4, ChancePercent = 13, Description = "Triple Points for 2 rounds" },
                new RewardOption { BuffType = BuffType.ExtraDice, Value = 1, Duration = 2, ChancePercent = 10, Description = "+1 Extra Dice for 2 rounds" },
                new RewardOption { BuffType = BuffType.ExtraDice, Value = 2, Duration = 2, ChancePercent = 1, Description = "+2 Extra Dice for 2 rounds" },

            }
        };
    }

    public void GiveReward(QuizReward rewardTier, Player player)
    {
        if (rewardTier == QuizReward.NoReward)
        {
            Debug.Log("No reward given");
            return;
        }

        var selectedReward = SelectRandomReward(rewardTier);
        player.PlayerBuffs.AddBuff(selectedReward.BuffType, selectedReward.Value, selectedReward.Duration);
        StartCoroutine(UIManager.Instance.DisplayRewardNotification(selectedReward.Description));
        Debug.Log($"{rewardTier}: {selectedReward.Description}");
    }

    private RewardOption SelectRandomReward(QuizReward tier)
    {
        var options = rewardTiers[tier];
        int randomPercent = Random.Range(1, 101); // 1 to 100
        
        int accumulatedChance = 0;
        foreach (var option in options)
        {
            accumulatedChance += option.ChancePercent;
            if (randomPercent <= accumulatedChance)
                return option;
        }
        
        return options[0]; // Fallback
    }
}

public enum QuizReward
{
    BigReward,
    MediumReward,
    SmallReward,
    NoReward
}