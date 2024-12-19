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

    [SerializeField]
    private GameObject directionPanel; // Panel that contains direction buttons

    [SerializeField]
    private Button directionButtonPrefab;

    [SerializeField]
    private GameObject homePromptPanel; // Panel for home tile prompt

    [SerializeField]
    private Button stayButton;

    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private TextMeshProUGUI diceResultText; // Text to display dice result

    private int totalResult = 0; // Tracks the total sum of dice rolls

    public static UnityAction<int> OnDiceTotal; // Event for dice total calculated + 2s delay

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        Dice.OnDiceResult += HandleDiceResult; // For getting results of dice rolls
        DiceThrower.OnAllDiceFinished += DisplayTotalResult; // For the bool of when all dice finish rolling
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= HandleDiceResult;
        DiceThrower.OnAllDiceFinished -= DisplayTotalResult;
    }

    private void HandleDiceResult(int diceResult)
    {
        totalResult += diceResult;
    }

    private async void DisplayTotalResult()
    {
        diceResultText?.SetText($"{totalResult}");
        await Task.Delay(1000);
        OnDiceTotal?.Invoke(totalResult);
        totalResult = 0; // Reset total result for next roll
        diceResultText?.SetText("");
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