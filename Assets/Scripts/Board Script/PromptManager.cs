using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator HandleHomeTile(Action<bool> onPlayerChoice)
    {
        bool? playerChoice = null;

        UIManager.Instance.ShowHomeTilePrompt((choice) => {
            playerChoice = choice;
        });

        yield return new WaitUntil(() => playerChoice != null);

        onPlayerChoice(playerChoice == true);
    }

    public IEnumerator HandlePvP(Action<bool> onPlayerChoice)
    {
        bool? playerChoice = null;

        UIManager.Instance.ShowPvPPrompt((choice) => {
            playerChoice = choice;
        });

        yield return new WaitUntil(() => playerChoice != null);

        onPlayerChoice(playerChoice == true);
    }

    public IEnumerator HandleDirections(List<Direction> availableDirections, Action<Direction> onDirectionChosen)
    {
        Direction? chosenDirection = null;

        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) => {
            chosenDirection = direction;
        });

        yield return new WaitUntil(() => chosenDirection != null);

        onDirectionChosen(chosenDirection.Value);
    }
}