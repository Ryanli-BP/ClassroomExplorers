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
                break;

            case GameState.PlayerTurnEnd:
                EndPlayerTurn();
                break;

            case GameState.GameEnd:
                EndGame();
                break;
        }

        OnGameStateChanged?.Invoke(currentState); //is this needed?
    }

    private void SetupGame()
    {
        SpawnPlayers();
        ChangeState(GameState.PlayerTurnStart);
    }

    private void SpawnPlayers()
    {
        PlayerManager.Instance.SpawnAllPlayersAtHome();
    }

    private void StartPlayerTurn()
    {

    }

    private void EnableDiceRoll()
    {

    }

    private void OnDiceRollComplete()
    {

    }

    private void StartPlayerMovement(int diceTotal)
    {

    }

    private void EndPlayerTurn()
    {

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
    PlayerTurnEnd,
    GameEnd
}
