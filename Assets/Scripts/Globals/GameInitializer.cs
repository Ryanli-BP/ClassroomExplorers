using UnityEngine;

public class GameInitializer : MonoBehaviour 
{
    public static GameInitializer Instance { get; private set; }
    public bool IsGameInitialized { get; private set; }
    public bool AllComponentsReady { get; private set; }
    public event System.Action OnGameInitialized;
    [SerializeField] private GameObject boardRoot;
    

    private bool isUIManagerReady;
    private bool isPlayerManagerReady;
    private bool isTileManagerReady;
    private bool isRoundManagerReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        // Automatically initialize game if running on PC instead of needing to wait for ARBoardPlacement
        if (PlatformUtils.IsRunningOnPC())
        {
            InitializeGame();
            boardRoot.gameObject.SetActive(true);
        }
    }

        private void CheckAllComponents()
    {
        AllComponentsReady = isUIManagerReady && 
                           isPlayerManagerReady && 
                           isTileManagerReady && 
                           isRoundManagerReady;
                           
        if (AllComponentsReady)
            Debug.Log("All components initialized!");
    }

    public void ConfirmManagerReady(string managerName)
    {
        switch (managerName)
        {
            case "UIManager":
                isUIManagerReady = true;
                break;
            case "PlayerManager":
                isPlayerManagerReady = true;
                break;
            case "TileManager":
                isTileManagerReady = true;
                break;
            case "RoundManager":
                isRoundManagerReady = true;
                break;
        }
        CheckAllComponents();
    }

    public void InitializeGame()
    {
        IsGameInitialized = true;
        OnGameInitialized?.Invoke();
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
