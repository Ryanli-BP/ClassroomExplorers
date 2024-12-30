using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject directionPanel; // Panel that contains direction buttons
    [SerializeField] private Button directionButtonPrefab;

    [SerializeField] private GameObject homePromptPanel; // Panel for home tile prompt
    [SerializeField] private Button stayButton;
    [SerializeField] private Button continueButton;

    [SerializeField] private TextMeshProUGUI diceResultText; // Text to display dice result
    [SerializeField] private List<TextMeshProUGUI> playerStatsTexts; // List of Texts to display each player's stats
    [SerializeField] private TextMeshProUGUI roundDisplayText; // Text to display the current round

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public async void DisplayDiceTotalResult(int totalResult)
    {
        diceResultText.text = $"{totalResult}";
        await Task.Delay(500); 
        diceResultText.text = "";
        GameManager.Instance.HandleDiceResultDisplayFinished();
    }

    public void UpdatePlayerStats(int playerID, int points, int level)
    {
        if (playerID > 0 && playerID <= playerStatsTexts.Count)
        {
            playerStatsTexts[playerID - 1].text = $"Player {playerID}: {points} Points, Level {level}";
        }
    }

    public void UpdateRound(int roundNumber)
    {
        roundDisplayText.text = $"Round {roundNumber}";
    }

    // Show direction choices at a crossroad
    public void ShowDirectionChoices(List<Direction> availableDirections, Action<Direction> onDirectionChosen)
    {
        directionPanel.SetActive(true);

        // Clear any existing buttons before showing again
        foreach (Transform child in directionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Direction direction in availableDirections)
        {
            Button newButton = Instantiate(directionButtonPrefab, directionPanel.transform);
            newButton.GetComponentInChildren<Text>().text = direction.ToString();

            // Add click listener to notify when this direction is chosen
            newButton.onClick.AddListener(() =>
            {
                directionPanel.SetActive(false);
                onDirectionChosen(direction); // Notify the PlayerMovement script
            });
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
}