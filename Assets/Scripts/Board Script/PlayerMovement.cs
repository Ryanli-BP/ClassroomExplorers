using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Tile currentTile; 
    private Direction _lastDirection; // To track the direction the player came from
    private bool isMoving = false;
    private bool initialMove; //One time flag for avoiding some actions intially for movement
    private int remainingSteps = 0;
    public event Action OnMovementComplete;
    private bool canFightPlayers;
    private bool canHealPlayers;
    private bool haveBoss;

    void Start()
    {
        ModeRules currentRules = GameConfigManager.Instance.GetCurrentRules();
        canFightPlayers = currentRules.canFightPlayers;
        canHealPlayers = currentRules.canHealPlayers;
        haveBoss = currentRules.haveBoss;
    }

    public Tile CurrentTile
    {
        get { return currentTile; }
        set { currentTile = value; }
    }

    public void MovePlayer(int diceroll)
    {
        if (isMoving || currentTile == null)
            return;

        Debug.Log($"Rolled: {diceroll} steps");
        remainingSteps = diceroll;
        StartCoroutine(MoveStepByStep());
    }

    private IEnumerator HandlePvPEncounter()
    {
        if (currentTile.TilePlayerIDs.Count > 0)
        Debug.Log($"Players on tile: [{string.Join(", ", currentTile.TilePlayerIDs)}]");
        {
            foreach (int tilePlayerID in currentTile.TilePlayerIDs)
            {
                Debug.Log($"TileID:{tilePlayerID} CurrID:{PlayerManager.Instance.CurrentPlayerID}");
                // Skip if it's the current player or if the player on tile is dead
                if (tilePlayerID == PlayerManager.Instance.CurrentPlayerID ||
                    PlayerManager.Instance.GetPlayerByID(tilePlayerID).Status != Status.Alive)
                {
                    continue;
                }

                Debug.Log($"Player {tilePlayerID} is on this tile.");
                bool? playerChoice = null;

                yield return StartCoroutine(PromptManager.Instance.HandlePvP((choice) => {
                    playerChoice = choice;
                }));

                while (playerChoice == null)
                {
                    yield return null;
                }

                if (playerChoice == true)
                {
                    Debug.Log("Player chose to fight.");
                    GameManager.Instance.OnCombatTriggered();
                    yield return StartCoroutine(CombatManager.Instance.HandleFight(PlayerManager.Instance.GetCurrentPlayer(), PlayerManager.Instance.GetPlayerByID(tilePlayerID)));
                    GameManager.Instance.IsResumingMovement = false;

                    remainingSteps = 0; // Stop movement after combat
                    
                    if (PlayerManager.Instance.GetCurrentPlayer().Status == Status.Dead)
                    {
                        isMoving = false;
                        yield break; // Exit if current player dies
                    }
                }
                else
                {
                    Debug.Log("Player chose to continue moving.");
                }
            }
        }
    }

    private IEnumerator HandleBossEncounter()
    {
        if (currentTile.BossOnTile)
        {
            Debug.Log("Boss encountered on tile");
            bool? playerChoice = null;

            yield return StartCoroutine(PromptManager.Instance.HandlePvP((choice) => {
                playerChoice = choice;
            }));

            while (playerChoice == null)
            {
                yield return null;
            }

            if (playerChoice == true)
            {
                Debug.Log("Player chose to fight boss.");
                GameManager.Instance.OnCombatTriggered();
                yield return StartCoroutine(CombatManager.Instance.HandleFight(PlayerManager.Instance.GetCurrentPlayer(), BossManager.Instance.activeBoss));
                GameManager.Instance.IsResumingMovement = false;

                remainingSteps = 0; // Stop movement after combat
                
                if (PlayerManager.Instance.GetCurrentPlayer().Status == Status.Dead)
                {
                    isMoving = false;
                    yield break;
                }
            }
            else
            {
                Debug.Log("Player chose to continue moving.");
            }
        }
    }

    private IEnumerator HandleHealEncounter()
    {
        if (currentTile.TilePlayerIDs.Count > 0)
        {
            foreach (int tilePlayerID in currentTile.TilePlayerIDs)
            {
                // Skip if it's the current player or if the player on tile is dead
                if (tilePlayerID == PlayerManager.Instance.CurrentPlayerID ||
                    PlayerManager.Instance.GetPlayerByID(tilePlayerID).Status != Status.Alive)
                {
                    continue;
                }

                Debug.Log($"Player {tilePlayerID} can be healed on this tile.");
                bool? playerChoice = null;

                yield return StartCoroutine(PromptManager.Instance.HandleHealing((choice) => {
                    playerChoice = choice;
                }));

                while (playerChoice == null)
                {
                    yield return null;
                }

                if (playerChoice == true)
                {
                    Debug.Log($"Player chose to heal Player {tilePlayerID}.");
                    Player otherPlayer = PlayerManager.Instance.GetPlayerByID(tilePlayerID);
                    otherPlayer.Heal(2);
                    remainingSteps = 0; // Stop movement after healing
                    break; // Exit after healing one player
                }
                else
                {
                    Debug.Log("Player chose to continue moving.");
                }
            }
        }
    }

    private IEnumerator HandleHomeTile(bool initialOnHome)
    {
        if (currentTile.GetTileType() == TileType.Home && 
            currentTile.GetHomePlayerID() == PlayerManager.Instance.CurrentPlayerID && 
            !initialOnHome)
        {
            Debug.Log("Reached home tile. Prompting player to choose.");
            yield return StartCoroutine(PromptManager.Instance.HandleHomeTile((choice) => {
                if (choice)
                {
                    Debug.Log("Player chose to stay on the home tile.");
                    isMoving = false;
                }
                else
                {
                    Debug.Log("Player chose to continue moving.");
                }
            }));
        }
    }

    private IEnumerator HandlePaths(List<Direction> availableDirections)
    {
        if (availableDirections.Count > 1)
        {
            Debug.Log("At a crossroad! Waiting for player to choose a direction...");
            List<Tile> highlightedTiles = TileManager.Instance.HighlightPossibleTiles(currentTile, remainingSteps);
            Direction? selectedDirection = null;
            
            yield return StartCoroutine(PromptManager.Instance.HandleDirections(availableDirections, (direction) => {
                selectedDirection = direction;
            }));

            if (selectedDirection.HasValue)
            {
                yield return StartCoroutine(MoveToNextTileCoroutine(selectedDirection.Value));
            }

            TileManager.Instance.ClearHighlightedTiles();
        }
        else
        {
            Direction nextDirection = availableDirections[0];
            yield return StartCoroutine(MoveToNextTileCoroutine(nextDirection));
        }
    }

    private IEnumerator MoveStepByStep()
    {
        isMoving = true;
        bool initialOnHome = true;
        initialMove = true;

        while (remainingSteps >= 0)
        {

            List<Direction> availableDirections = currentTile.GetAllAvailableDirections();
            Debug.Log($"remaining steps:{remainingSteps}");
            yield return StartCoroutine(UIManager.Instance.DisplayRemainingDiceSteps(remainingSteps));
            
            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }

            if (!initialMove)
            {
                if (canFightPlayers)
                {
                    yield return StartCoroutine(HandlePvPEncounter());
                }
                else if (canHealPlayers)
                {
                    yield return StartCoroutine(HandleHealEncounter());
                }
                
                if (haveBoss)
                {
                    yield return StartCoroutine(HandleBossEncounter());
                }

            }
            
            //Finishes handling all movement actions on final tile
            if (remainingSteps == 0)
            {
                isMoving = false;
            }

            if (!isMoving) break;

            yield return StartCoroutine(HandleHomeTile(initialOnHome));
            if (!isMoving) { break; }
            initialOnHome = false;

            yield return StartCoroutine(HandlePaths(availableDirections));

            remainingSteps--;
            yield return new WaitForSeconds(0.05f);

            if (initialMove) //currently, initialMove is a condition for pvpencounter
            {
                initialMove = false;
            }
        }

        isMoving = false;
        initialMove = true;

            OnMovementComplete?.Invoke();      
    }


    private IEnumerator MoveToNextTileCoroutine(Direction direction)
    {
        _lastDirection = direction;
        Tile nextTile = currentTile.GetConnectedTile(direction);

        if (nextTile != null)
        {
            if (currentTile != null)
            {
                currentTile.RemovePlayer(PlayerManager.Instance.CurrentPlayerID);
            }

            nextTile.AddPlayer(PlayerManager.Instance.CurrentPlayerID);
            currentTile = nextTile;
            MovementAnimation movementAnimation = PlayerManager.Instance.GetCurrentPlayer().GetComponent<MovementAnimation>();
            yield return StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
        else
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
        }
    }
}