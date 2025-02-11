using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimateClean;
using System.Collections;

[System.Serializable]
public class PlayerStatsUI
{
    public GameObject pointsBar; //points text is included in bar
    public TextMeshProUGUI levelText;
    public GameObject healthBar;
}

[DefaultExecutionOrder(0)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject directionPanel; // Panel that contains direction buttons
    [SerializeField] private Button northButton;
    [SerializeField] private Button eastButton;
    [SerializeField] private Button southButton;
    [SerializeField] private Button westButton;

    [SerializeField] private GameObject homePromptPanel; // Panel for home tile prompt
    [SerializeField] private Button stayButton;
    [SerializeField] private Button continueButton;

    [SerializeField] private GameObject pvpPromptPanel; // Panel for PvP prompt
    [SerializeField] private Button fightButton;
    [SerializeField] private Button continueMovingButton;

    [SerializeField] private GameObject centreDisplayPanel;
    [SerializeField] private List<GameObject> playerUIPrefab = new List<GameObject>();
    [SerializeField] private List<PlayerStatsUI> playerStatsUIList = new List<PlayerStatsUI>();
    [SerializeField] private GameObject BossHealthBarPanel;
    [SerializeField] private TextMeshProUGUI roundDisplayText;
    [SerializeField] private TextMeshProUGUI currentTurnText; 

    [SerializeField] private TextMeshProUGUI reviveCounterText;
    private Dictionary<int, string> playerReviveMessages = new Dictionary<int, string>();

    [SerializeField] public GameObject rollDiceButtonPanel;
    [SerializeField] public GameObject evadeButtonPanel;
    [SerializeField] public Button rollDiceButton; // Button to roll the dice
    [SerializeField] public Button evadeButton; // button for evade option

    [SerializeField] private GameObject starPrefab; // Reference to the star prefab
    [SerializeField] private TextMeshProUGUI damageTextPrefab;
    [SerializeField] private Canvas mainCanvas; // Reference to the main canvas

    [SerializeField] private GameObject healPromptPanel;
    [SerializeField] private Button healButton;
    [SerializeField] private Button skipHealButton;

    [SerializeField] private GameObject boardUI;

    private Vector3 lastPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    private void Start()
    {
        if (GameInitializer.Instance.IsGameInitialized) //if game initialized, initialize UI immediately
        {
            InitializeUI();
        }
        else //if not, wait for game to initialize(AR Tap to Place Board)
        {
            GameInitializer.Instance.OnGameInitialized += OnGameInitialized;
        }

        GameInitializer.Instance.ConfirmManagerReady("UIManager");
    }

    private void InitializeUI()
    {
        StartCoroutine(WaitForManagersToInitialize());
    }

    private void OnGameInitialized()
    {
        GameInitializer.Instance.OnGameInitialized -= OnGameInitialized;
        InitializeUI();
    }

    private IEnumerator WaitForManagersToInitialize() //This is not strictly needed but it's safer this way
    {
        // Wait for all required managers
        while (PlayerManager.Instance == null || 
               GameManager.Instance == null ||
               TileManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Initialize UI elements
        GenereateUIList();
        SetupButtonListeners();
        SetInitialPanelStates();
    }

    private void SetupButtonListeners()
    {
        if (rollDiceButton != null)
            rollDiceButton.onClick.AddListener(OnRollDiceButtonClicked);
        if (evadeButton != null)
            evadeButton.onClick.AddListener(OnEvadeButtonClicked);
    }

    private void SetInitialPanelStates()
    {
        if (directionPanel) directionPanel.SetActive(false);
        if (homePromptPanel) homePromptPanel.SetActive(false);
        if (pvpPromptPanel) pvpPromptPanel.SetActive(false);
        if (centreDisplayPanel) centreDisplayPanel.SetActive(false);
        if (rollDiceButtonPanel) rollDiceButtonPanel.gameObject.SetActive(false);
        if (evadeButtonPanel) evadeButtonPanel.gameObject.SetActive(false);
    }

    private void SetAvatarInactive()
    {
        for(int i = 0; i < playerUIPrefab.Count; i++)
        {
            playerUIPrefab[i].SetActive(false);
        }
    }

    private void GenereateUIList()
    {
        if (PlayerManager.Instance == null) //for debugging purposes
        {
            Debug.LogError("PlayerManager.Instance is null!");
            return;
        }

        if (playerUIPrefab == null || playerUIPrefab.Count == 0)
        {
            Debug.LogError("playerUIPrefab list is null or empty!");
            return;
        }

        SetAvatarInactive();
        
        int numPlayers = PlayerManager.Instance.numOfPlayers;

        for(int i = 0; i < numPlayers && i < playerUIPrefab.Count; i++)
        {
            if (playerUIPrefab[i] != null)
            {
                playerUIPrefab[i].SetActive(true);
            }
            else
            {
                Debug.LogError($"Player UI Prefab at index {i} is null!");
            }
        }
    }

    private void OnDestroy()
    {
        if (GameInitializer.Instance != null)
            GameInitializer.Instance.OnGameInitialized -= OnGameInitialized;
    }



    private void OnRollDiceButtonClicked()
    {
        DiceManager.Instance.RollDice();
    }

    private void OnEvadeButtonClicked()
    {
        DiceManager.Instance.RollDice();
    }

    public void SetRollDiceButtonVisibility(bool isVisible)
    {
        rollDiceButtonPanel.gameObject.SetActive(isVisible);
    }

    public void SetEvadeButtonVisibility(bool isVisible)
    {
        evadeButtonPanel.gameObject.SetActive(isVisible);
    }

    public void SetRollDiceButtonText(string text)
    {
        rollDiceButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public async void DisplayDiceTotalResult(int totalResult)
    {
        centreDisplayPanel.SetActive(true);
        centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"{totalResult}";
        await Task.Delay(500); 
        centreDisplayPanel.SetActive(false);
        GameManager.Instance.HandleDiceResultDisplayFinished();
    }

    public async void DisplayPointChange(int pointsChange)
    {
        centreDisplayPanel.SetActive(true);
        string originalColor = centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().color.ToHexString(); // Store the original color
        string colorTag = pointsChange > 0 ? "<color=#FFFF9B>" : "<color=#8BBFFF>";
        string symbol = pointsChange > 0 ? "+" : "";

        centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"{colorTag}{symbol}{pointsChange}</color>";
        await Task.Delay(500); 
        centreDisplayPanel.SetActive(false);
        centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().color = originalColor.ToColor(); // Restore the original color
    }

    public async void DisplayLevelUp()
    {
        centreDisplayPanel.SetActive(true);
        centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Level Up!";
        await Task.Delay(500); 
        centreDisplayPanel.SetActive(false);
    }

    public void UpdatePlayerPoints(int playerIndex, int points, int levelUpPoints)
    {
        var pointsAnimation = playerStatsUIList[playerIndex - 1].pointsBar.GetComponent<PointsAnimation>();
        if (pointsAnimation != null)
        {
            pointsAnimation.AnimatePoints(points, levelUpPoints);
        }
        else
        {
            Debug.LogError("PointsAnimation component missing in player stats UI");
        }
    }

    public void UpdatePlayerLevel(int playerIndex, int level)
    {
        playerStatsUIList[playerIndex - 1].levelText.text = $"LV <#FF6573>{level}</color>";
    }

    public void UpdatePlayerHealth(int playerIndex, int health)
    {
        var healthAnimation = playerStatsUIList[playerIndex - 1].healthBar.GetComponent<HealthAnimation>();
        if (healthAnimation != null)
        {
            healthAnimation.AnimateHealth(health, Player.MAX_HEALTH);
        }
        else
        {
            Debug.LogError("HealthAnimation component missing in player stats UI");
        }
    }

    public void UpdateBossHealth( int health)
    {
        var healthAnimation = BossHealthBarPanel.transform.Find("healthBar").GetComponent<HealthAnimation>();
        if (healthAnimation != null)
        {
            healthAnimation.AnimateHealth(health, Boss.MAX_HEALTH);
        }
        else
        {
            Debug.LogError("HealthAnimation component missing in Boss healthbar");
        }
    }

    public void ToggleBossUI(bool active)
    {
        BossHealthBarPanel.SetActive(active);
    }

    public void DisplayGainStarAnimation(int playerIndex)
    {
        var playerPointBar = playerStatsUIList[playerIndex - 1].pointsBar.GetComponent<RectTransform>();
        GameObject starInstance = Instantiate(starPrefab, mainCanvas.transform); // Instantiate as child of the main canvas
        PointStarAnimation pointStarAnimation = starInstance.GetComponent<PointStarAnimation>();
        pointStarAnimation.AnimatePointStar(starInstance, playerPointBar);
    }

    public void DisplayLoseStarAnimation()
    {
        GameObject starInstance = Instantiate(starPrefab, mainCanvas.transform); // Instantiate as child of the main canvas
        PointStarAnimation pointStarAnimation = starInstance.GetComponent<PointStarAnimation>();
        pointStarAnimation.AnimateLosePointStar(starInstance);
    }

    public async void DisplayDamageNumber(Vector3 position, int damage)
    {
        TextMeshProUGUI damageText = Instantiate(damageTextPrefab, mainCanvas.transform);
        damageText.text = $"-{damage}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
        damageText.transform.position = screenPos + new Vector3(0, 50, 0);

        // Longer duration and delayed fade
        float duration = 1f;
        float fadeStartTime = duration * 0.5f; // Start fading halfway through
        float startTime = Time.time;
        Color textColor = damageText.color;

        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            damageText.transform.position += Vector3.up * Time.deltaTime * 50f;

            // Only fade in the second half of the animation
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) * 1f; // Normalize fade progress to 0-1
                textColor.a = 1 - fadeProgress;
                damageText.color = textColor;
            }

            await Task.Yield();
        }

        Destroy(damageText.gameObject);
    }

    public void UpdateRound(int roundNumber)
    {
        roundDisplayText.text = $"Round {roundNumber}";
    }
    
    public void UpdateCurrentPlayerTurn(int playerID)
    {
        currentTurnText.text = $"Player {playerID}'s Turn";
    }

    public void DisplayBossTurn()
    {
        currentTurnText.text = "Boss's Turn";
    }

    public void UpdateReviveCounter(int playerID, int reviveCounter)
    {
        if (reviveCounter > 0)
        {
            string message = $"Player {playerID} Revives in: {reviveCounter} Rounds\n";
            playerReviveMessages[playerID] = message;
        }
        else
        {
            playerReviveMessages.Remove(playerID);
        }
        UpdateReviveCounterText();
    }

    public void ClearReviveCounter(int playerID)
    {
        playerReviveMessages.Remove(playerID);
        UpdateReviveCounterText();
    }

    private void UpdateReviveCounterText()
    {
        reviveCounterText.text = string.Join("\n", playerReviveMessages.Values);
    }

    // Show direction choices at a crossroad
    public void ShowDirectionChoices(List<Direction> availableDirections, Action<Direction> onDirectionChosen)
        {
            directionPanel.SetActive(true);

            // Hide all direction buttons initially
            northButton.gameObject.SetActive(false);
            eastButton.gameObject.SetActive(false);
            southButton.gameObject.SetActive(false);
            westButton.gameObject.SetActive(false);

            // Show only the necessary buttons
            foreach (Direction direction in availableDirections)
            {
                Button button = null;
                switch (direction)
                {
                    case Direction.North:
                        button = northButton;
                        break;
                    case Direction.East:
                        button = eastButton;
                        break;
                    case Direction.South:
                        button = southButton;
                        break;
                    case Direction.West:
                        button = westButton;
                        break;
                }

                if (button != null)
                {
                    button.gameObject.SetActive(true);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        directionPanel.SetActive(false);
                        onDirectionChosen(direction);
                    });
                }
            }
        }

    // Show prompt when player reaches a home tile
    public void ShowHomeTilePrompt(Action<bool> onPlayerChoice)
    {
        homePromptPanel.SetActive(true);
        stayButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();

        stayButton.onClick.AddListener(() => {
            homePromptPanel.SetActive(false);
            onPlayerChoice(true);
        });

        continueButton.onClick.AddListener(() => {
            homePromptPanel.SetActive(false);
            onPlayerChoice(false);
        });
    }

    public void ShowPvPPrompt(Action<bool> onPlayerChoice)
    {
        pvpPromptPanel.SetActive(true);
        fightButton.onClick.RemoveAllListeners();
        continueMovingButton.onClick.RemoveAllListeners();

        fightButton.onClick.AddListener(() => {
            pvpPromptPanel.SetActive(false);
            onPlayerChoice(true);
        });

        continueMovingButton.onClick.AddListener(() => {
            pvpPromptPanel.SetActive(false);
            onPlayerChoice(false);
        });
    }

    public void ShowHealingPrompt(Action<bool> onPlayerChoice)
    {
        healPromptPanel.SetActive(true);
        healButton.onClick.RemoveAllListeners();
        skipHealButton.onClick.RemoveAllListeners();

        healButton.onClick.AddListener(() => {
            healPromptPanel.SetActive(false);
            onPlayerChoice(true);
        });

        skipHealButton.onClick.AddListener(() => {
            healPromptPanel.SetActive(false);
            onPlayerChoice(false);
        });
    }

    public void SetBoardUIActive(bool active)
    {
        if (boardUI != null)
        {
            Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
            lastPos = currentPlayer.transform.position;
            Debug.Log($"Last position: {lastPos}");
            boardUI.SetActive(active);
            if (currentPlayer.transform.position != lastPos)
            {
                throw new System.Exception("Player position changed while board UI was active");
            }
        }
        else
        {
            Debug.LogError("Board UI reference missing in UIManager");
        }
    }
}
public static class ColorExtensions
{
    public static string ToHexString(this Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }

    public static Color ToColor(this string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString($"#{hex}", out color);
        return color;
    }
}