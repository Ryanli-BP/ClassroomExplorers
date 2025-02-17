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
    [SerializeField] private TextMeshProUGUI centreDisplayText;
    [SerializeField] private TextMeshProUGUI BonusDisplayText;
    [SerializeField] private List<GameObject> playerUIPrefab = new List<GameObject>();
    [SerializeField] private List<PlayerStatsUI> playerStatsUIList = new List<PlayerStatsUI>();
    [SerializeField] private GameObject BossHealthBarPanel;
    [SerializeField] private TextMeshProUGUI roundDisplayText;
    [SerializeField] private TextMeshProUGUI currentTurnText; 
    [SerializeField] private GameObject NotificationBar;
    [SerializeField] private TextMeshProUGUI NotificationText;

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
        yield return new WaitUntil(() => GameInitializer.Instance.IsManagerReady("PlayerManager"));

        // Initialize UI elements
        GenereateUIList();
        SetupButtonListeners();
        SetInitialPanelStates();

        foreach(Player player in PlayerManager.Instance.GetPlayerList())
        {
            StartCoroutine(player.InitializePlayerUI());
        }
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
        
        int numPlayers = GameConfigManager.Instance.numOfPlayers;

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

    private int bonusValue = 0; //Combat and star bonuses

    public void SetBonusUIValue(int bonus)
    {
        Debug.Log($"Bonus value set to {bonus}");
        bonusValue = bonus;
    }

    public IEnumerator DisplayDiceTotalResult(int diceResult)
    {
        centreDisplayPanel.SetActive(true);
        centreDisplayText.text = $"{diceResult}";
        yield return new WaitForSeconds(0.5f);

        if (bonusValue > 0)
        {
            yield return StartCoroutine(DisplayBonusValue(diceResult, "+"));
        }

        centreDisplayPanel.SetActive(false);
        GameManager.Instance.HandleDiceResultDisplayFinished();
    }

    public IEnumerator DisplayBonusValue(int baseResult, string operation)
    {
        Color currentColor = centreDisplayText.color;
        BonusDisplayText.color = currentColor;
        
        string displayOperation = operation == "*" ? "X" : operation; //either * or +, if * then display X
        BonusDisplayText.text = $"{displayOperation}{bonusValue}";
        yield return new WaitForSeconds(0.5f);

        int finalResult = operation switch
        {
            "*" => baseResult * bonusValue,
            "+" => baseResult + bonusValue,
            _ => baseResult + bonusValue
        };

        BonusDisplayText.text = "";
        centreDisplayText.text = $"{finalResult}";
        yield return new WaitForSeconds(0.5f);
        
        bonusValue = 0;
    }

    public IEnumerator DisplayPointChange(int pointsChange)
    {
        centreDisplayPanel.SetActive(true);
        Color originalColor = centreDisplayText.color;
        
        // Set color based on positive/negative points
        centreDisplayText.color = pointsChange > 0 ? 
            new Color(1f, 1f, 0.61f) : // #FFFF9B (yellow)
            new Color(0.545f, 0.749f, 1f); // #8BBFFF (blue)
        
        string symbol = pointsChange > 0 ? "+" : "";
        centreDisplayText.text = $"{symbol}{pointsChange}";
        yield return new WaitForSeconds(0.5f);

        if (bonusValue > 1)
        {
            yield return StartCoroutine(DisplayBonusValue(pointsChange, "*"));
        }
        
        centreDisplayPanel.SetActive(false);
        centreDisplayText.color = originalColor;
    }

    public IEnumerator DisplayLevelUp()
    {
        centreDisplayPanel.SetActive(true);
        centreDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Level Up!";
        yield return new WaitForSeconds(0.5f);
        centreDisplayPanel.SetActive(false);
    }

    public IEnumerator UpdatePlayerPoints(int playerIndex, int points, int levelUpPoints)
    {
        var pointsAnimation = playerStatsUIList[playerIndex - 1].pointsBar.GetComponent<PointsAnimation>();
        if (pointsAnimation != null)
        {
            yield return StartCoroutine(pointsAnimation.AnimatePoints(points, levelUpPoints));
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

    public IEnumerator DisplayDamageNumber(Vector3 position, int damage)
    {
        TextMeshProUGUI damageText = Instantiate(damageTextPrefab, mainCanvas.transform);
        damageText.text = $"-{damage}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
        damageText.transform.position = screenPos + new Vector3(0, 50, 0);

        float duration = 1f;
        float startTime = Time.time;
        Color textColor = damageText.color;

        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            damageText.transform.position += Vector3.up * Time.deltaTime * 50f;

            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) * 1f;
                textColor.a = 1 - fadeProgress;
                damageText.color = textColor;
            }

            yield return null;
        }

        Destroy(damageText.gameObject);
    }

    public IEnumerator DisplayRewardNotification(string message)
    {
        NotificationBar.SetActive(true);
        NotificationText.text = message;
        yield return new WaitForSeconds(2f);
        NotificationBar.SetActive(false);
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
            boardUI.SetActive(active);
            Debug.Log($"Board UI is now {(active ? "active" : "inactive")}");
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