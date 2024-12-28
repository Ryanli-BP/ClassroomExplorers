using UnityEngine;
using System;

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
        currentState = newState;
        Debug.Log($"Changing state to: {newState}");

        switch (currentState)
        {
            case GameState.GameSetup:
                SetupGame();
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
                Debug.Log("IN PLAYER FINISHED MOVING STATE");
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
        DiceManager.OnAllDiceFinished += OnDiceRollComplete;
    }

    private void OnDiceRollComplete()
    {
        DiceManager.OnAllDiceFinished -= OnDiceRollComplete;
        int totalDiceResult = DiceManager.Instance.GetTotalDiceResult();
        UIManager.Instance.DisplayTotalResult(totalDiceResult);
        UIManager.OnDiceResultDisplayFinished += HandleDiceResultDisplayFinished; // Subscribe to the event
    }

    private void HandleDiceResultDisplayFinished()
    {
        UIManager.OnDiceResultDisplayFinished -= HandleDiceResultDisplayFinished; // Unsubscribe from the event
        ChangeState(GameState.PlayerMoving);
    }

    private void StartPlayerMovement()
    {
        PlayerManager.Instance.StartPlayerMovement(DiceManager.Instance.GetTotalDiceResult());
        ChangeState(GameState.PlayerFinishedMoving);
        Debug.Log("CHANGING TO PLAYER FINISHED MOVING STATE");
    }

    private void StartTileAction()
    {
        Debug.Log("IN TILE ACTION STATE");
        TileManager.Instance.getTileAction(PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>().CurrentTile);
        ChangeState(GameState.PlayerTurnEnd);
    }

    private void EndPlayerTurn()
    {
        Debug.Log($"Player {PlayerManager.Instance.GetCurrentPlayer().getPlayerID()}'s turn ended.");
        PlayerManager.Instance.GetNextPlayer();
        ChangeState(GameState.PlayerTurnStart);
    }

    private void EndGame()
    {
        Debug.Log("Game Over! Implement end-game logic here.");
    }
}

public enum GameState
{
    GameSetup,
    PlayerTurnStart,
    PlayerRollingDice,
    PlayerMoving,
    PlayerFinishedMoving,
    PlayerTurnEnd,
    GameEnd
}
