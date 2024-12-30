using UnityEngine;
using System;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState currentState;
    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.GameSetup);
    }

    private void ChangeState(GameState newState)
{
    Debug.Log($"Changing state from {currentState} to {newState}");
    currentState = newState;
    OnStateChanged(newState);
}

    private void OnStateChanged(GameState newState)
    {
        Debug.Log($"State changed to {newState}");

        switch (currentState)
        {
            case GameState.GameSetup:
                SetupGame();
                break;

            case GameState.RoundStart:
                RoundEvent();
                break;

            case GameState.PlayerTurnStart:
                StartPlayerTurn();
                break;

            case GameState.PlayerRollingDice:
                EnableDiceRoll();
                break;

            case GameState.PlayerMoving:
                StartPlayerMovement();
                break;

            case GameState.PlayerFinishedMoving:
                StartTileAction();
                break;

            case GameState.PlayerTurnEnd:
                EndPlayerTurn();
                break;

            case GameState.GameEnd:
                EndGame();
                break;
        }

        OnGameStateChanged?.Invoke(currentState);
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    private void SetupGame()
    {
        PlayerManager.Instance.SpawnAllPlayersAtHome();
        ChangeState(GameState.RoundStart);
    }

    private void RoundEvent()
    {
        Debug.Log("Starting new round.");
        RoundManager.Instance.IncrementRound();
        RoundManager.Instance.GiveRoundPoints();
        ChangeState(GameState.PlayerTurnStart);
    }

    private void StartPlayerTurn()
    {
        Debug.Log($"Player {PlayerManager.Instance.GetCurrentPlayer().getPlayerID()}'s turn.");
        ChangeState(GameState.PlayerRollingDice);
    }

    private void EnableDiceRoll()
    {
        DiceManager.Instance.EnableDiceRoll();
    }

    public void OnDiceRollComplete()
    {
        int totalDiceResult = DiceManager.Instance.GetTotalDiceResult();
        UIManager.Instance.DisplayDiceTotalResult(totalDiceResult);
    }

    public void HandleDiceResultDisplayFinished()
    {
        ChangeState(GameState.PlayerMoving);
    }

    private void StartPlayerMovement()
    {
        Debug.Log("Starting player movement");
        PlayerMovement playerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        playerMovement.OnMovementComplete += OnPlayerMovementComplete;
        PlayerManager.Instance.StartPlayerMovement(DiceManager.Instance.GetTotalDiceResult());
    }

    private void OnPlayerMovementComplete()
    {
        Debug.Log("Player movement complete");
        PlayerMovement playerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        playerMovement.OnMovementComplete -= OnPlayerMovementComplete;
        ChangeState(GameState.PlayerFinishedMoving);
    }


    private void StartTileAction()
    {
        TileManager.Instance.getTileAction(PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>().CurrentTile);
        ChangeState(GameState.PlayerTurnEnd);
    }

    private void EndPlayerTurn()
    {
        Debug.Log($"Player {PlayerManager.Instance.GetCurrentPlayer().getPlayerID()}'s turn ended.");
        PlayerManager.Instance.GoNextPlayer();
        RoundManager.Instance.IncrementTurn();

        if (RoundManager.Instance.Turn == 1)
        {
            ChangeState(GameState.RoundStart);
        }
        else
        {
            ChangeState(GameState.PlayerTurnStart);
        }
    }

    public void FinalLevelAchieved()
    {
        ChangeState(GameState.GameEnd);
    }
    private void EndGame()
    {
        Debug.Log("Game Over! Implement end-game logic here.");
    }
}

public enum GameState
{
    GameSetup,
    RoundStart,
    PlayerTurnStart,
    PlayerRollingDice,
    PlayerMoving,
    PlayerFinishedMoving,
    PlayerTurnEnd,
    GameEnd
}
