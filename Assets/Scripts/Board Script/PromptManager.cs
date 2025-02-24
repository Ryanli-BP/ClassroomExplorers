using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class PromptManager : MonoBehaviourPunCallbacks
{

    public Direction? ChosenDirection { get; private set; }


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
        Direction? chosenDirection = null;
        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) => {
            chosenDirection = direction;
            // Broadcast the choice to all clients so everyone updates
            Debug.Log(PlayerManager.Instance.GetCurrentPlayer().getPlayerID());
            Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
            PromptManager.Instance.photonView.RPC("RPCDirectionChosen", RpcTarget.All, (int)direction);

        });


        yield return new WaitUntil(() => chosenDirection != null);
        
        onDirectionChosen(chosenDirection.Value);
    }


    [PunRPC]
    public void RPCDirectionChosen(int directionValue)
    {
        Direction direction = (Direction)directionValue;

        Debug.Log($"Direction chosen: {direction}");
    }

}