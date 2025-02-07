using UnityEngine;
using System;
using System.Collections;

[DefaultExecutionOrder(20)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState currentState;
    public static event Action<GameState> OnGameStateChanged;
    private bool isBossTurn = false;

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

                if (isBossTurn)
                {
                    DiceManager.Instance.RollDice();
                }
                break;

            case GameState.PlayerMovement:
                if (isBossTurn)
                {
                    StartBossMovement();
                }
                else
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
                StartCoroutine(EndPlayerTurn());
                break;

            case GameState.BossTurn:
                StartBossTurn();
                break;
            
            case GameState.BossTurnEnd:
                HandleBossTurnEnd();
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

    public void SetupGame()
    {
        StartCoroutine(WaitForComponentsAndSetup());
    }

    private IEnumerator WaitForComponentsAndSetup() //To ensure everything is setup on other scripts first
    {
        while (!GameInitializer.Instance.AllComponentsReady)
        {
            yield return new WaitForSeconds(0.1f);
        }

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

    public void StartBossTurn()
    {
        UIManager.Instance.DisplayBossTurn();
        isBossTurn = true;
        ChangeState(GameState.PlayerRollingMovementDice);
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

    public void HandleBossTurnEnd()
    {
        UIManager.Instance.UpdateCurrentPlayerTurn(RoundManager.Instance.Turn);
        isBossTurn = false;
        ChangeState(GameState.RoundStart);
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
        PlayerMovement PlayerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>(); 
        PlayerMovement.OnMovementComplete -= OnPlayerMovementComplete; //unsubscribe from action becuase OnPlayerMovementComplete is not called if land on quiz tile
        ChangeState(GameState.PlayerTurnEnd);
    }


    private void StartPlayerMovement()
    {
        Debug.Log("Starting player movement");
        PlayerMovement PlayerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        PlayerMovement.OnMovementComplete += OnPlayerMovementComplete;
        PlayerManager.Instance.StartPlayerMovement(DiceManager.Instance.GetTotalDiceResult());
    }

    private void StartBossMovement()
    {
        Debug.Log("Starting boss movement");
        BossMovement BossMovement = BossManager.Instance.activeBoss.Movement;
        BossMovement.OnMovementComplete += OnBossMovementComplete;
        BossManager.Instance.StartBossMovement(DiceManager.Instance.GetTotalDiceResult());
    }

    private void OnPlayerMovementComplete()
    {
        Debug.Log("Player movement complete");
        PlayerMovement PlayerMovement = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>();
        PlayerMovement.OnMovementComplete -= OnPlayerMovementComplete;
        ChangeState(GameState.PlayerFinishedMoving);
    }

    private void OnBossMovementComplete()
    {
        Debug.Log("Boss movement complete");
        BossMovement BossMovement = BossManager.Instance.activeBoss.Movement;
        BossMovement.OnMovementComplete -= OnBossMovementComplete;
        ChangeState(GameState.BossTurnEnd);
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

    private IEnumerator EndPlayerTurn()
    {
        yield return StartCoroutine(slightDelay(0.5f));

        Debug.Log($"Player {PlayerManager.Instance.GetCurrentPlayer().getPlayerID()}'s turn ended.");
        PlayerManager.Instance.GoNextPlayer();
        RoundManager.Instance.IncrementTurn();

        if (RoundManager.Instance.Turn == 1)
        {
            // All players have taken their turn, now it's boss turn
            if (GameConfigManager.Instance.GetCurrentRules().haveBoss)
            {
                ChangeState(GameState.BossTurn);
            }
            else
            {
                ChangeState(GameState.RoundStart);
            }
        }
        else
        {
            ChangeState(GameState.PlayerTurnStart);
        }
    }

    private IEnumerator slightDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
    BossTurn,
    BossTurnEnd,
    PlayerLandQuiz, 
    PlayerEndQuiz,
    GameEnd
}
