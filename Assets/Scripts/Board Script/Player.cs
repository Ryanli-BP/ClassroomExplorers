using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;
using Photon.Pun;

public enum Status { Alive, Dead }

public class Player : Entity, IPunObservable
{
    [SerializeField] private int playerID;

    public const int REVIVAL_COUNT = 5;
    public int MAX_HEALTH = 6;
    public const int MAX_LEVEL = 5;
    public const int MAX_TROPHY = 5;
    public GameObject[] bodyColors; 
    public GameObject[] hats;
    public int Points { get; set; }
    public int Level { get; set; } = 0;
    public int TrophyCount { get; set; } = 0;
    public int QuizStreak { get; set; } = 0;
    [SerializeField] private PlayerBuffs playerBuffs = new PlayerBuffs();
    public PlayerBuffs PlayerBuffs => playerBuffs;

    public int ReviveCounter { get; set; } = 0;
    void Awake()
    {
        if (playerID <= GameConfigManager.Instance.numOfPlayers)
        {
            Points = 0;
            Level = 0;
            TrophyCount = 0;
            Health = MAX_HEALTH;
            Status = Status.Alive;

            /*Buffs.AddBuff(BuffType.AttackUp, 2, 2); //for test
            Buffs.AddBuff(BuffType.DefenseUp, 1, 2); //for test
            Buffs.AddBuff(BuffType.EvadeUp, 3, 2); //for test
            Buffs.AddBuff(BuffType.DoublePoints, 2, 2); //for test
            Buffs.AddBuff(BuffType.ExtraDice, 1, 2); //for test*/
        }
        if (GameConfigManager.Instance == null)
        {
            Debug.Log("GameConfigManager is null");
        }
    }

     public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerID);
            stream.SendNext(Points);
            stream.SendNext(Level);
            stream.SendNext(TrophyCount);
            stream.SendNext(Health);
            stream.SendNext((int)Status);
            stream.SendNext(transform.position);
        }
        else
        {
            playerID = (int)stream.ReceiveNext();
            Points = (int)stream.ReceiveNext();
            Level = (int)stream.ReceiveNext();
            TrophyCount = (int)stream.ReceiveNext();
            Health = (int)stream.ReceiveNext();
            Status = (Status)stream.ReceiveNext();
            transform.position = (Vector3)stream.ReceiveNext();
            
            // Update UI after receiving new values
            StartCoroutine(InitializePlayerUI());
        }
    }
    public string GetPlayerNickName()
    {
        // Find the Photon player that matches this player's ID
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == playerID)
            {
                // Return the nickname from custom properties if it exists, otherwise use the default nickname
                return p.CustomProperties.ContainsKey("Nickname") 
                    ? (string)p.CustomProperties["Nickname"] 
                    : p.NickName;
            }
        }
        
        // Fallback in case no matching player is found
        return $"Player {playerID}";
    }
    public IEnumerator InitializePlayerUI()
    {
        Debug.Log($"Initialize playerUI {playerID}");

        // Update UI once UIManager is ready
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);

        if(GameConfigManager.Instance.CurrentMode == GameMode.COOP)
        {
            UIManager.Instance.ChangePlayerUIforMode(playerID, GameMode.COOP); //hides trophy and show level
            UIManager.Instance.UpdatePlayerLevel(playerID, Level);
            yield return StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(Level)));
        }
        else
        {
            UIManager.Instance.ChangePlayerUIforMode(playerID, GameMode.FFA);//hides level and show trophy
            UIManager.Instance.UpdatePlayerTrophy(playerID, TrophyCount);
            yield return StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(TrophyCount)));
        }
    }
    
 [PunRPC]
public void SetPlayerAppearance(int selectedBodyIndex, int selectedHatIndex)
{
    Debug.Log($"SetPlayerAppearance called for player {playerID} with body:{selectedBodyIndex}, hat:{selectedHatIndex}");
    
    // Initialize arrays if needed
    InitializeCustomizationArrays();
    
    // Set body color
    if (bodyColors != null && bodyColors.Length > selectedBodyIndex)
    {
        for (int i = 0; i < bodyColors.Length; i++)
        {
            if (bodyColors[i] != null)
            {
                bodyColors[i].SetActive(i == selectedBodyIndex);
            }
        }
    }
    else
    {
        Debug.LogError($"Invalid body color index {selectedBodyIndex} or bodyColors not initialized");
    }

    // Set hat
    if (hats != null && hats.Length > selectedHatIndex)
    {
        for (int i = 0; i < hats.Length; i++)
        {
            if (hats[i] != null)
            {
                hats[i].SetActive(i == selectedHatIndex);
            }
        }
    }
    else
    {
        Debug.LogError($"Invalid hat index {selectedHatIndex} or hats not initialized");
    }
}

private void InitializeCustomizationArrays()
{
    if (bodyColors == null || bodyColors.Length == 0)
    {
        Transform bodyParent = transform.Find("Mesh Object/Bone_Body");
        if (bodyParent != null)
        {
            bodyColors = new GameObject[bodyParent.childCount];
            for (int i = 0; i < bodyParent.childCount; i++)
            {
                bodyColors[i] = bodyParent.GetChild(i).gameObject;
            }
        }
    }

    if (hats == null || hats.Length == 0)
    {
        Transform hatParent = transform.Find("hats");
        if (hatParent != null)
        {
            hats = new GameObject[hatParent.childCount];
            for (int i = 0; i < hatParent.childCount; i++)
            {
                hats[i] = hatParent.GetChild(i).gameObject;
            }
        }
    }
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

    public IEnumerator AddPoints(int amount)
    {
        Points = Math.Max(0, Points + amount);
        Debug.Log($"Player {playerID} now has {Points} points.");
        int playerMilestone = GameConfigManager.Instance.CurrentMode == GameMode.COOP ? Level : TrophyCount;
        yield return StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(playerMilestone)));
    }

    public override void AddBuff(BuffType type, int value, int duration)
    {
        playerBuffs.AddBuff(type, value, duration);
    }

    public override void UpdateBuffDurations()
    {
        playerBuffs.UpdateBuffDurations();
    }

    public void EarnTrophy()
    {
        if (TrophyCount >= MAX_TROPHY)
        {
            Debug.Log("Player reach MAX_TROPHY");
            return;
        }
        
        TrophyCount += 1;
        Debug.Log($"Player {playerID} earned a trophy.");
        UIManager.Instance.UpdatePlayerTrophy(playerID, TrophyCount);
        StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(TrophyCount)));
        StartCoroutine(UIManager.Instance.DisplayEarnTrophy(playerID, TrophyCount));
    }

    public void LevelUp()
    {
        if (Level >= MAX_LEVEL)
        {
            Debug.Log("Player reach MAX_LEVEL");
            return;
        }
        
        Level += 1;
        Debug.Log($"Player {playerID} leveled up to level {Level}.");
        UIManager.Instance.UpdatePlayerLevel(playerID, Level);
        StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(Level)));
        StartCoroutine(UIManager.Instance.DisplayLevelUp(playerID, Level));

        // Add random permanent buff
        BuffType[] possibleBuffs = { BuffType.AttackUp, BuffType.DefenseUp, BuffType.EvadeUp };
        BuffType randomBuff = possibleBuffs[UnityEngine.Random.Range(0, possibleBuffs.Length)];
        AddBuff(randomBuff, 2, 100); // Value of 2, duration of 100 for "permanent" effect
        Debug.Log($"Player {playerID} gained permanent {randomBuff} buff");

        //Gain 1 HP
        MAX_HEALTH++;
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);
    }

    public override void LoseHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log($"Player {playerID} now has {Health} health.");
        UIManager.Instance.UpdatePlayerHealth(playerID, Health);

        if (Health == 0)
        {
            Dies();
        }
    }

    public override void Dies()
    {
        Status = Status.Dead;
        PlayerManager.Instance.DeadPlayers.Add(this);
        UIManager.Instance.UpdateReviveCounter(playerID, REVIVAL_COUNT - ReviveCounter);

        Points /= 2;
        int playerMilestone = GameConfigManager.Instance.CurrentMode == GameMode.COOP ? Level : TrophyCount;
        StartCoroutine(UIManager.Instance.UpdatePlayerPoints(playerID, Points, PlayerManager.Instance.GetMilestonePoints(playerMilestone)));
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
        Vector3 teleportPosition = new Vector3(position.x, position.y + 0.7f * ARBoardPlacement.worldScale, position.z);
        
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

[System.Serializable]
public class PlayerBuffs : EntityBuffs
{
    public bool TriplePoints => activeBuffs.Any(b => b.Type == BuffType.TriplePoints);
    public bool DoublePoints => activeBuffs.Any(b => b.Type == BuffType.DoublePoints) && !TriplePoints;
}