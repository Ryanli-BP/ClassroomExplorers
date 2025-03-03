using UnityEngine;
using System;
using System.Collections;

[DefaultExecutionOrder(20)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState currentState;
    public static event Action<GameState> OnGameStateChanged;
    public bool isBossTurn {get; private set;} = false;

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
                StartCoroutine(RoundEvent());
                break;

            case GameState.PlayerTurnStart:
                StartPlayerTurn();
                if (PlatformUtils.IsRunningOnPC())
                {
                    CameraManager.Instance.SetFollowTarget(PlayerManager.Instance.GetCurrentPlayer().transform);
                }
                break;

            case GameState.RollingMovementDice:

                if (isBossTurn)
                {
                    DiceManager.Instance.EnableDiceRoll(true);
                    DiceManager.Instance.RollDice(); //automatical dice roll for Boss
                }
                else
                {
                    DiceManager.Instance.EnableDiceRoll(false);
                }
                break;

            case GameState.BoardMovement:
                if (!IsResumingMovement)
                {
                    if (isBossTurn)
                        StartBossMovement();
                    else
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

            case GameState.FinishedMoving:
                StartCoroutine(StartTileAction());
                break;

            case GameState.PlayerTurnEnd:
                StartCoroutine(EndPlayerTurn());
                break;

            case GameState.BossTurn:
                StartBossTurn();
                if (PlatformUtils.IsRunningOnPC() && BossManager.Instance.activeBoss != null)
                {
                    CameraManager.Instance.SetFollowTarget(BossManager.Instance.activeBoss.transform);
                }
                break;
            
            case GameState.BossTurnEnd:
                HandleBossTurnEnd();
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

    private IEnumerator RoundEvent()
    {
        Debug.Log("Starting new round.");
        RoundManager.Instance.IncrementRound();
        yield return StartCoroutine(RoundManager.Instance.GiveRoundPoints());
        RoundManager.Instance.ApplyRoundBuff();

        QuizManager.Instance.StartNewQuiz();

        yield return new WaitUntil(() => QuizManager.Instance.OnQuizComplete);

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
            ChangeState(GameState.RollingMovementDice);
        }
        
    }

    public void StartBossTurn()
    {
        UIManager.Instance.DisplayBossTurn();
        isBossTurn = true;
        ChangeState(GameState.RollingMovementDice);
    }

    public IEnumerator OnDiceRollComplete()
    {
        int totalDiceResult = DiceManager.Instance.GetTotalDiceResult();
        yield return StartCoroutine(UIManager.Instance.DisplayDiceTotalResult(totalDiceResult));

        if (currentState == GameState.PlayerCombat && OnDiceRollResultForCombat != null)
        {
            OnDiceRollResultForCombat.Invoke(totalDiceResult);
            OnDiceRollResultForCombat = null; // Clear the callback to prevent multiple invocations.
        }
    }

    public void HandleDiceResultDisplayFinished()
    {
        if (currentState == GameState.RollingMovementDice)
        {
            ChangeState(GameState.BoardMovement);
        }
        else if (currentState == GameState.PlayerCombat)
        {
            OnDiceResultDisplayForCombat?.Invoke(true); // Trigger display completion callback
            OnDiceResultDisplayForCombat = null; // Reset the callback
        }
    }

    public void HandleCombatEnd()
    {
        ChangeState(GameState.BoardMovement);
    }

    public void HandleBossTurnEnd()
    {
        UIManager.Instance.UpdateCurrentPlayerTurn(RoundManager.Instance.Turn);
        isBossTurn = false;
        ChangeState(GameState.RoundStart);
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
        ChangeState(GameState.FinishedMoving);
    }

    private void OnBossMovementComplete()
    {
        Debug.Log("Boss movement complete");
        BossMovement BossMovement = BossManager.Instance.activeBoss.Movement;
        BossMovement.OnMovementComplete -= OnBossMovementComplete;
        ChangeState(GameState.FinishedMoving);
    }

    public void OnCombatTriggered()
    {
        Debug.Log("Combat triggered! Transitioning to Combat state.");
        ChangeState(GameState.PlayerCombat);
    }

    private IEnumerator StartTileAction()
    {
        TileManager.Instance.OnTileActionComplete = false;

        if(isBossTurn)
        {
            TileManager.Instance.getBossTileAction(BossManager.Instance.activeBoss.Movement.CurrentTile);
        }
        else
        {
            StartCoroutine(TileManager.Instance.getPlayerTileAction(PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovement>().CurrentTile));
        }

        // Wait until the action completes
        yield return new WaitUntil(() => TileManager.Instance.OnTileActionComplete);
   

        // Early break for special cases
        if (currentState == GameState.GameEnd || currentState == GameState.RollingMovementDice)
            yield break;

        // Change state based on turn
        GameState nextState = isBossTurn ? GameState.BossTurnEnd : GameState.PlayerTurnEnd;
        ChangeState(nextState);
    }

    private IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(0.5f);

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

    public void HandleReroll()
    {
        Debug.Log("Player landed on reroll tile - starting new roll");
        IsResumingMovement = false;
        ChangeState(GameState.RollingMovementDice);
    }

    public void WinGameConditionAchieved()
    {
        ChangeState(GameState.GameEnd);
    }
    private void EndGame()
    {
        UIManager.Instance.DisplayGameEnd();
        Debug.Log("Game Over! Implement end-game logic here.");
    }
}

public enum GameState
{
    GameSetup,
    RoundStart,
    PlayerTurnStart,
    RollingMovementDice,
    BoardMovement,
    PlayerCombat,
    FinishedMoving,
    PlayerTurnEnd,
    BossTurn,
    BossTurnEnd,
    GameEnd
}
