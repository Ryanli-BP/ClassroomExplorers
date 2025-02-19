using UnityEngine;
using System.Collections.Generic;

public enum GameMode
{
    FFA,
    TEAM,
    COOP
}

public enum QuizMode
{
    NORMAL,
    BUZZ,
    TIME_RUSH
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
    [SerializeField] private QuizMode currentQuizMode = QuizMode.NORMAL;
    [SerializeField] private int _numOfPlayers = 2; // Backing field
    [SerializeField] private int _quizTimeLimit = 30; // Backing field
    public int numOfPlayers
    {
        get => _numOfPlayers;
        private set => _numOfPlayers = value;
    }
    public int quizTimeLimit
    {
        get => _quizTimeLimit;
        private set => _quizTimeLimit = value;
    }
    
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

    public QuizMode CurrentQuizMode
    {
        get => currentQuizMode;
        set => currentQuizMode = value;
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

    public int GetNumOfPlayers()
    {
        return numOfPlayers;
    }

    public ModeRules GetCurrentRules()
    {
        return modeRules[currentMode];
    }
}