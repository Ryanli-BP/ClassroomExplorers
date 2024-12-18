using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<GameObject> playerPrefabs; 
    [SerializeField] private Transform playerParent;
    [SerializeField] private DiceThrower diceThrower;

    private List<PlayerMovement> players = new List<PlayerMovement>();
    private int currentPlayerIndex = 0;
    private GameState currentState;

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
                // Wait for player movement to finish
                break;

            case GameState.PlayerTurnEnd:
                EndPlayerTurn();
                break;

            case GameState.GameEnd:
                EndGame();
                break;
        }
    }

    private void SetupGame()
    {
        SpawnPlayers();
        ChangeState(GameState.PlayerTurnStart);
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            
            Tile homeTile = TileManager.Instance.allTiles.Find(tile =>
            {
                Home homeComponent = tile.GetComponent<Home>();
                return homeComponent != null && homeComponent.playerID == i;
            });

            if (homeTile != null)
            {
                GameObject newPlayer = Instantiate(playerPrefabs[i], playerParent);

                PlayerMovement playerMovement = newPlayer.GetComponent<PlayerMovement>();
                playerMovement.Initialize(homeTile, i);

                players.Add(playerMovement);
            }
            else
            {
                Debug.LogError($"No home tile found for player {i}!");
            }
        }
    }

    private void StartPlayerTurn()
    {
        Debug.Log($"Player {currentPlayerIndex}'s turn!");
        ChangeState(GameState.PlayerRollingDice);
    }

    private void EnableDiceRoll()
    {
        if (diceThrower == null)
        {
            Debug.LogError("DiceThrower is not assigned in the GameManager!");
            return;
        }

        Debug.Log("Waiting for dice roll...");
        diceThrower.enabled = true;
        DiceThrower.OnAllDiceFinished += OnDiceRollComplete;
    }

    private void OnDiceRollComplete()
    {
        DiceThrower.OnAllDiceFinished -= OnDiceRollComplete;

        int diceTotal = diceThrower.GetDiceTotal();
        Debug.Log($"Player {currentPlayerIndex} rolled a {diceTotal}!");

        diceThrower.enabled = false; 
        StartPlayerMovement(diceTotal);
    }

    private void StartPlayerMovement(int diceTotal)
    {
        Debug.Log($"Player {currentPlayerIndex} starts moving.");
        players[currentPlayerIndex].MovePlayer(diceTotal);
        ChangeState(GameState.PlayerMoving);

        
        StartCoroutine(WaitForPlayerMovement(players[currentPlayerIndex]));
    }

    private System.Collections.IEnumerator WaitForPlayerMovement(PlayerMovement currentPlayer)
    {
        while (currentPlayer.IsMoving())
        {
            yield return null; 
        }

        ChangeState(GameState.PlayerTurnEnd);
    }

    private void EndPlayerTurn()
    {
        Debug.Log($"Player {currentPlayerIndex} finished their turn.");
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count; 
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
    PlayerTurnEnd,
    GameEnd
}
