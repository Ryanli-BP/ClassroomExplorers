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
    public Dictionary<GameMode, GameConfig> modes;
}

[System.Serializable]
public class GameConfig
{
    // External configurable settings
    public bool enabled;
    public int maxPlayers; 
    public float roundTime;
    public int playersPerTeam;
    public float respawnTime;
}

[System.Serializable]
public class ModeRules
{
    // Mode-specific gameplay rules
    public bool canHealPlayers;
    public bool canFightPlayers;
    public bool friendlyFire;
}

public class GameConfigManager : MonoBehaviour
{
    [SerializeField] private string configUrl = "https://your-api.com/gamemodes";
    private GameModeConfig currentConfig;
    private Dictionary<GameMode, ModeRules> modeRules;
    private bool isInitialized;

    public static GameConfigManager Instance { get; private set; }

    private void Awake()
    {
        InitializeModeRules();
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

    private void InitializeModeRules()
    {
        modeRules = new Dictionary<GameMode, ModeRules>
        {
            { GameMode.FFA, new ModeRules 
                { 
                    canHealPlayers = false, 
                    canFightPlayers = true, 
                    friendlyFire = true 
                }
            },
            { GameMode.TEAM, new ModeRules 
                { 
                    canHealPlayers = true, 
                    canFightPlayers = true, 
                    friendlyFire = false 
                }
            },
            { GameMode.COOP, new ModeRules 
                { 
                    canHealPlayers = true, 
                    canFightPlayers = false, 
                    friendlyFire = false 
                }
            }
        };
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
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load fallback config: {e.Message}");
        }
    }

    public GameConfig GetCurrentConfig()
    {
        return currentConfig.modes[currentConfig.currentMode];
    }

    public ModeRules GetCurrentRules()
    {
        return modeRules[currentConfig.currentMode];
    }

    public async Task RefreshConfig()
    {
        await LoadConfigFromServer();
    }
}