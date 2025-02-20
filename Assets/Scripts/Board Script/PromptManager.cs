using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class PromptManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView photonView;

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

    public IEnumerator HandleHealing(Action<bool> onPlayerChoice)
    {
        bool? playerChoice = null;

        UIManager.Instance.ShowHealingPrompt((choice) => {
            playerChoice = choice;
        });

        yield return new WaitUntil(() => playerChoice != null);

        onPlayerChoice(playerChoice == true);
    }

    public IEnumerator HandleDirections(List<Direction> availableDirections, Action<Direction> onDirectionChosen)
    {
        // Only show UI to active player
        if (!PlayerManager.Instance.GetCurrentPlayer().photonView.IsMine)
        {
            yield return new WaitUntil(() => GameManager.Instance.GetCurrentState() != GameState.BoardMovement);
            yield break;
        }

        Direction? chosenDirection = null;
        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) => {
            chosenDirection = direction;
            // Sync choice across network
            photonView.RPC("RPCDirectionChosen", RpcTarget.All, (int)direction);
        });

        yield return new WaitUntil(() => chosenDirection != null);
        onDirectionChosen(chosenDirection.Value);
    }
}