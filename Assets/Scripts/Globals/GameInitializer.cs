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
    private bool isQuizManagerReady; // Add this line

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
                           isRoundManagerReady &&
                           isQuizManagerReady;
                           
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
            case "QuizManager": // Add this case
                isQuizManagerReady = true;
                break;
        }
        CheckAllComponents();
    }

    public void InitializeGame()
    {
        IsGameInitialized = true;
        OnGameInitialized?.Invoke();
    }

    public bool IsManagerReady(string managerName)
    {
        switch (managerName)
        {
            case "UIManager":
                return isUIManagerReady;
            case "PlayerManager":
                return isPlayerManagerReady;
            case "TileManager":
                return isTileManagerReady;
            case "RoundManager":
                return isRoundManagerReady;
            case "QuizManager": // Add this case
                return isQuizManagerReady;
            default:
                return false;
        }
    }
}


