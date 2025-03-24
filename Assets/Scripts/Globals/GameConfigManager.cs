using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

// Defines the available game modes
public enum GameMode
{
    FFA,    // Free-For-All mode
    TEAM,   // Team-based mode
    COOP    // Cooperative mode
}

// Defines the available quiz modes
public enum QuizMode
{
    NORMAL,     // Standard quiz mode
    BUZZ,       // Buzzer-style quiz mode
    TIME_RUSH   // Time-based quiz mode
}

// Contains rules and settings for each game mode
[System.Serializable]
public class ModeRules
{
    public bool canHealPlayers;  // Whether players can heal each other
    public bool canFightPlayers; // Whether PvP combat is enabled
    public bool haveBoss;        // Whether the mode includes a boss enemy
    public bool friendlyFire;    // Whether friendly fire is enabled
}

public class GameConfigManager : MonoBehaviour
{
    // Serialized fields for Unity Inspector
    [SerializeField] private GameMode currentMode = GameMode.COOP;
    [SerializeField] private QuizMode currentQuizMode = QuizMode.NORMAL;
    [SerializeField] private int _numOfPlayers = 4; // Backing field for player count
    [SerializeField] private int _quizTimeLimit = 30; // Backing field for quiz time limit
    [SerializeField] private int boardNumber = 2; // Backing field for quiz time limit

    // Public boolean to indicate if fetch operation is complete
    public bool IsFetchComplete { get; private set; } = false;

    // Dictionary to store mode-specific rules
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

    public int numOfPlayers
    {
        get => _numOfPlayers;
        private set => _numOfPlayers = value;
    }

    // Properties for quiz time limit
    public int quizTimeLimit
    {
        get => _quizTimeLimit;
        private set => _quizTimeLimit = value;
    }

    // Properties for board number
    public int BoardNumber
    {
        get => boardNumber;
        private set => boardNumber = value;
    }

    // Properties for current game mode
    public GameMode CurrentMode
    {
        get => currentMode;
        set => currentMode = value;
    }

    // Properties for current quiz mode
    public QuizMode CurrentQuizMode
    {
        get => currentQuizMode;
        set => currentQuizMode = value;
    }

    public ModeRules GetCurrentRules()
    {
        return modeRules[currentMode];
    }

    // Initialize rules for each game mode
    private void InitializeModeRules()
    {
        modeRules = new Dictionary<GameMode, ModeRules>
        {
            // Free-For-All mode rules
            { GameMode.FFA, new ModeRules
                {
                    canHealPlayers = false,
                    canFightPlayers = true,
                    friendlyFire = true,
                    haveBoss = false
                }
            },
            // Team mode rules
            { GameMode.TEAM, new ModeRules
                {
                    canHealPlayers = true,
                    canFightPlayers = true,
                    friendlyFire = false,
                    haveBoss = false
                }
            },
            // Cooperative mode rules
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


    // Data class for deserializing API response
    [System.Serializable]
    private class ConfigData
    {
        public int timeLimit;
        public GameMode teamMode;
        public int numberOfPlayers;
        public QuizMode quizMode;
        public int boardNumber;
    }

    // Start fetching config data when the component starts
    private void Start()
    {
        StartCoroutine(FetchConfigData());
    }

    // Fetch configuration data from API
    private IEnumerator FetchConfigData()
    {
        string url = "http://127.0.0.1:8000/api/v1.0.0/config/get-config/";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning($"Error fetching config data: {request.error}");
            IsFetchComplete = true;
        }
        else
        {
            ConfigData configData = JsonUtility.FromJson<ConfigData>(request.downloadHandler.text);
            ApplyConfigData(configData);
        }
    }

    // Apply fetched configuration data to game settings
    private void ApplyConfigData(ConfigData configData)
    {
        quizTimeLimit = configData.timeLimit;
        currentMode = configData.teamMode;
        numOfPlayers = configData.numberOfPlayers;
        currentQuizMode = configData.quizMode;
        boardNumber = configData.boardNumber;
        IsFetchComplete = true;
    }
}

public static class PlatformUtils
{
    public static bool IsRunningOnPC()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.OSXPlayer ||
               Application.platform == RuntimePlatform.LinuxPlayer ||
               Application.platform == RuntimePlatform.WindowsEditor ||
               Application.platform == RuntimePlatform.OSXEditor;
    }
}