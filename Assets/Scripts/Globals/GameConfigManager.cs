using UnityEngine;
using System.Collections.Generic;

public enum GameMode
{
    FFA,
    TEAM,
    COOP
}

[System.Serializable]
public class ModeRules
{
    public bool canHealPlayers;
    public bool canFightPlayers;
    public bool haveBoss; 
    public bool friendlyFire;
}

public class GameConfigManager : MonoBehaviour
{
    [SerializeField] private GameMode currentMode = GameMode.FFA;
    
    private Dictionary<GameMode, ModeRules> modeRules;

    public static GameConfigManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeModeRules();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameMode CurrentMode 
    {
        get => currentMode;
        set => currentMode = value;
    }

    private void InitializeModeRules()
    {
        modeRules = new Dictionary<GameMode, ModeRules>
        {
            { GameMode.FFA, new ModeRules 
                { 
                    canHealPlayers = false, 
                    canFightPlayers = true, 
                    friendlyFire = true, 
                    haveBoss = false
                }
            },
            { GameMode.TEAM, new ModeRules 
                { 
                    canHealPlayers = true, 
                    canFightPlayers = true, 
                    friendlyFire = false,
                    haveBoss = false 
                }
            },
            { GameMode.COOP, new ModeRules 
                { 
                    canHealPlayers = true, 
                    canFightPlayers = false, 
                    friendlyFire = false,
                    haveBoss = true 
                }
            }
        };
    }

    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
    }

    public ModeRules GetCurrentRules()
    {
        return modeRules[currentMode];
    }
}