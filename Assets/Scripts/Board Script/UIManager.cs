using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using UltimateClean;

[System.Serializable]
public class PlayerStatsUI
{
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI levelText;
    public GameObject healthBar;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private HealthAnimation healthAnimation;

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

    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private List<PlayerStatsUI> playerStatsUIList = new List<PlayerStatsUI>();
    [SerializeField] private TextMeshProUGUI roundDisplayText;
    [SerializeField] private TextMeshProUGUI currentPlayerTurnText; 

    [SerializeField] private TextMeshProUGUI reviveCounterText;
    private Dictionary<int, string> playerReviveMessages = new Dictionary<int, string>();

    [SerializeField] public GameObject rollDiceButtonPanel;
    [SerializeField] public GameObject evadeButtonPanel;
    [SerializeField] public Button rollDiceButton; // Button to roll the dice
    [SerializeField] public Button evadeButton; // button for evade option


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        rollDiceButton.onClick.AddListener(OnRollDiceButtonClicked);
        evadeButton.onClick.AddListener(OnEvadeButtonClicked);
        rollDiceButtonPanel.gameObject.SetActive(false);
        evadeButtonPanel.gameObject.SetActive(false);
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
        diceResultText.text = $"{totalResult}";
        await Task.Delay(500); 
        diceResultText.text = "";
        GameManager.Instance.HandleDiceResultDisplayFinished();
    }

    public void UpdatePlayerPoints(int playerIndex, int points)
    {
        playerStatsUIList[playerIndex].pointsText.text = $"P {points}";
    }

    public void UpdatePlayerLevel(int playerIndex, int level)
    {
        playerStatsUIList[playerIndex].levelText.text = $"LV <#FF6573>{level}</color>";
    }

    public void UpdatePlayerHealth(int playerIndex, int health)
    {
        var healthAnimation = playerStatsUIList[playerIndex].healthBar.GetComponent<HealthAnimation>();
        if (healthAnimation != null)
        {
            healthAnimation.AnimateHealth(health, Player.MAX_HEALTH);
        }
    }

    public void UpdateRound(int roundNumber)
    {
        roundDisplayText.text = $"Round {roundNumber}";
    }
    
    public void UpdateCurrentPlayerTurn(int playerID)
    {
        currentPlayerTurnText.text = $"Player {playerID}'s Turn";
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
}