using UnityEngine;
using System;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState currentState;
    public static event Action<GameState> OnGameStateChanged;

    public bool IsResumingMovement { get; set; } = false; // Flag such that player don't StartPlayerMovement again after combat->moving state change
    public Action<int> OnDiceRollResultForCombat;
    public Action<bool> OnDiceResultDisplayForCombat; // New action to notify when dice result display finishes.



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

            case GameState.PlayerRollingMovementDice:
                EnableMovementDiceRoll();
                break;

            case GameState.PlayerMovement:
                if (!IsResumingMovement)
                {
                    StartPlayerMovement();
                }
                else
                {
                    Debug.Log("Resuming movement");
                }
                break;

            case GameState.PlayerCombat:
                IsResumingMovement = true;
                break;

            case GameState.PlayerFinishedMoving:
                StartTileAction();
                break;

            case GameState.PlayerTurnEnd:
                EndPlayerTurn();
                break;
            
            case GameState.PlayerLandQuiz:
                HandleQuizStart();
                break;
            
            case GameState.PlayerEndQuiz:
                HandleQuizEnd();
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
        if (PlayerManager.Instance.GetCurrentPlayer().Status == Status.Dead)
        {
            Debug.Log("Player is dead. Skipping turn.");
            ChangeState(GameState.PlayerTurnEnd);
            return;
        }
        else
        {
            ChangeState(GameState.PlayerRollingMovementDice);
        }
        
    }

    private void EnableMovementDiceRoll()
    {
        DiceManager.Instance.EnableDiceRoll();
    }

    public void OnDiceRollComplete()
    {
        int totalDiceResult = DiceManager.Instance.GetTotalDiceResult();
        UIManager.Instance.DisplayDiceTotalResult(totalDiceResult);

        if (currentState == GameState.PlayerCombat && OnDiceRollResultForCombat != null)
        {
            OnDiceRollResultForCombat.Invoke(totalDiceResult);
            OnDiceRollResultForCombat = null; // Clear the callback to prevent multiple invocations.
        }
    }

    public void HandleDiceResultDisplayFinished()
    {
        if (currentState == GameState.PlayerRollingMovementDice)
        {
            ChangeState(GameState.PlayerMovement);
        }
        else if (currentState == GameState.PlayerCombat)
        {
            OnDiceResultDisplayForCombat?.Invoke(true); // Trigger display completion callback
            OnDiceResultDisplayForCombat = null; // Reset the callback
        }
    }

    public void HandleCombatEnd()
    {
        ChangeState(GameState.PlayerMovement);
    }

    public void HandleQuizLand()
    {
    ChangeState(GameState.PlayerLandQuiz);
    }   

    public void HandleQuizStart()
    {
        UIManager.Instance.SetBoardUIActive(false);
        QuizManager.Instance.StartNewQuiz();
    }

    public void HandleQuizEnd()
    {
        UIManager.Instance.SetBoardUIActive(true);
        ChangeState(GameState.PlayerTurnEnd);
    }


    private void StartPlayerMovement()
    {
        Debug.Log("Starting player movement");
        PlayerMovement PlayerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        PlayerMovement.OnMovementComplete += OnPlayerMovementComplete;
        PlayerManager.Instance.StartPlayerMovement(DiceManager.Instance.GetTotalDiceResult());
    }

    private void OnPlayerMovementComplete()
    {
        Debug.Log("Player movement complete");
        PlayerMovement PlayerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        PlayerMovement.OnMovementComplete -= OnPlayerMovementComplete;
        ChangeState(GameState.PlayerFinishedMoving);
    }

    public void OnCombatTriggered()
    {
        Debug.Log("Combat triggered! Transitioning to Combat state.");
        ChangeState(GameState.PlayerCombat);
    }

    private void StartTileAction()
    {
        TileManager.Instance.getTileAction(PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>().CurrentTile);

        if (currentState == GameState.GameEnd) //if reached final level from home tile
        {
            return;
        }

        if (currentState == GameState.PlayerRollingMovementDice) //if land on reroll tile
        {
            return;
        }

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

    public void HandleReroll()
    {
        Debug.Log("Player landed on reroll tile - starting new roll");
        IsResumingMovement = false;
        ChangeState(GameState.PlayerRollingMovementDice);
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
    PlayerRollingMovementDice,
    PlayerMovement,
    PlayerCombat,
    PlayerFinishedMoving,
    PlayerTurnEnd,
    PlayerLandQuiz, 
    PlayerInQuiz,
    PlayerEndQuiz,
    GameEnd
}
