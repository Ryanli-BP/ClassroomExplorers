using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum GameMode
{
    FFA,
    TEAM,
    COOP
}

[System.Serializable]
public class GameModeConfig
{
    public GameMode currentMode;
    public Dictionary<GameMode, ModeSettings> modes;
}

[System.Serializable]
public class ModeSettings
{
    public bool enabled;
    public int maxPlayers;
    public float respawnTime;
    public int maxTeams;
    public int playersPerTeam;
    public bool friendlyFire;
    public int roundTime;
    public float warmupTime;
    public int minPlayers;
}

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private string configUrl = "https://your-api.com/gamemodes";
    private GameModeConfig currentConfig;
    private bool isInitialized;

    public static GameModeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await InitializeGameModes();
    }

    private async Task InitializeGameModes()
    {
        await LoadConfigFromServer();
        isInitialized = true;
    }

    private async Task LoadConfigFromServer()
    {
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(configUrl))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    currentConfig = JsonConvert.DeserializeObject<GameModeConfig>(json);
                    ApplyGameMode(currentConfig.currentMode);
                }
                else
                {
                    Debug.LogError($"Config fetch failed: {request.error}");
                    LoadFallbackConfig();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading config: {e.Message}");
            LoadFallbackConfig();
        }
    }

    private void LoadFallbackConfig()
    {
        try
        {
            TextAsset fallbackConfig = Resources.Load<TextAsset>("fallback_config");
            currentConfig = JsonConvert.DeserializeObject<GameModeConfig>(fallbackConfig.text);
            ApplyGameMode(currentConfig.currentMode);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load fallback config: {e.Message}");
        }
    }

    public void ApplyGameMode(GameMode mode)
    {
        if (!isInitialized)
        {
            Debug.LogError("GameModeManager not initialized yet");
            return;
        }

        if (!currentConfig.modes.ContainsKey(mode))
        {
            Debug.LogError($"Game mode {mode} not found in config");
            return;
        }

        if (!currentConfig.modes[mode].enabled)
        {
            Debug.LogError($"Game mode {mode} is disabled");
            return;
        }

        currentConfig.currentMode = mode;
        ModeSettings settings = currentConfig.modes[mode];
        ApplyModeSettings(settings);
    }

    private void ApplyModeSettings(ModeSettings settings)
    {
        switch (currentConfig.currentMode)
        {
            case GameMode.FFA:
                ApplyFFASettings(settings);
                break;
            case GameMode.TEAM:
                ApplyTeamSettings(settings);
                break;
            case GameMode.COOP:
                ApplyCompetitiveSettings(settings);
                break;
        }
    }

    private void ApplyFFASettings(ModeSettings settings)
    {
        // Implement FFA specific settings

    }

    private void ApplyTeamSettings(ModeSettings settings)
    {
        // Implement Team specific settings

    }

    private void ApplyCompetitiveSettings(ModeSettings settings)
    {
        // Implement Competitive specific settings

    }

    public GameMode GetCurrentMode()
    {
        return currentConfig.currentMode;
    }

    public ModeSettings GetCurrentModeSettings()
    {
        return currentConfig.modes[currentConfig.currentMode];
    }

    public async Task RefreshConfig()
    {
        await LoadConfigFromServer();
    }
}