using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Tile currentTile; // Assign the starting tile in the Inspector
    private Direction FacingDirection; // To track the direction the player came from
    [SerializeField] private Direction _lastDirection; // To track the direction the player came from
    private bool isMoving = false;
    private bool initialMove = true; //One time flag for removing TilePlayerID
    private int remainingSteps = 0;
    public event Action OnMovementComplete;

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

    private IEnumerator MoveStepByStep()
    {
        isMoving = true;
        bool initialOnHome = true; //one time flag

        while (remainingSteps > 0)
        {
            if (initialMove)
            {
                // Reset the TilePlayerID of the current tile
                currentTile.TilePlayerID = 0;
                initialMove = false;
            }

            // Get valid directions based on the last direction         
            List<Direction> availableDirections = currentTile.GetAllAvailableDirections(_lastDirection);
            
            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }
            
            if (currentTile.TilePlayerID != 0 && currentTile.TilePlayerID != PlayerManager.Instance.CurrentPlayerID) {
                Debug.Log($"Player {currentTile.TilePlayerID} is on this tile.");

                bool? playerChoice = null; // Use nullable bool to track the choice

                yield return StartCoroutine(PromptManager.Instance.HandlePvP((choice) => {
                    playerChoice = choice; 
                }));

                // Wait until the player's choice is resolved
                while (playerChoice == null) {
                    yield return null;
                }

                if (playerChoice == true) {
                    Debug.Log("Player chose to fight.");
                    GameManager.Instance.OnCombatTriggered();
                    yield return StartCoroutine(EventManager.Instance.HandleFight(currentTile.TilePlayerID, PlayerManager.Instance.CurrentPlayerID));
                    GameManager.Instance.IsResumingMovement = false; // needed for states to work correctly after combat
                } else {
                    Debug.Log("Player chose to continue moving.");
                }
            }


            // Prompt the player if they reach their home tile
            if (currentTile.GetTileType() == TileType.Home  && currentTile.GetHomePlayerID() == PlayerManager.Instance.CurrentPlayerID && !initialOnHome)
            {
                Debug.Log("Reached home tile. Prompting player to choose.");
                yield return StartCoroutine(PromptManager.Instance.HandleHomeTile((choice) => {
                    if (choice)
                    {
                        Debug.Log("Player chose to stay on the home tile.");
                        isMoving = false;
                        OnMovementComplete?.Invoke();
                    }
                    else
                    {
                        Debug.Log("Player chose to continue moving.");
                    }
                }));
            }

            initialOnHome = false;

            if (!isMoving) { yield break; } //if player chooses to stop

            // If at a crossroads, stop and wait for player input
            if (availableDirections.Count > 1)
            {
                Debug.Log("At a crossroad! Waiting for player to choose a direction...");
                List<Tile> highlightedTiles = TileManager.Instance.HighlightPossibleTiles(currentTile, remainingSteps);
                yield return StartCoroutine(PromptManager.Instance.HandleDirections(availableDirections, (direction) => {
                    MoveToNextTile(direction);
                }));
                TileManager.Instance.ClearHighlightedTiles();
            }
            else
            {
                // Move in the only available direction
                Direction nextDirection = availableDirections[0];
                MoveToNextTile(nextDirection);
            }

            remainingSteps--;
            yield return new WaitForSeconds(0.05f); // Optional delay for smoother movement
        }

        isMoving = false;
        initialMove = true;
        OnMovementComplete?.Invoke();
    }

    private void MoveToNextTile(Direction direction)
    {
        // Update last direction based on the current movement direction
        _lastDirection = direction;

        Vector3 targetPosition = currentTile.transform.position;

        // Move the player in the chosen direction (assumes tiles are spaced 1 unit apart)
        switch (direction)
        {
            case Direction.North:
                targetPosition += new Vector3(0, 0, 1);
                break;
            case Direction.East:
                targetPosition += new Vector3(1, 0, 0);
                break;
            case Direction.South:
                targetPosition += new Vector3(0, 0, -1);
                break;
            case Direction.West:
                targetPosition += new Vector3(-1, 0, 0);
                break;
        }

        // Find the tile at the new position
        currentTile = TileManager.Instance.GetTileAtPosition(targetPosition);

        if (currentTile != null)
        {
            // Apply movement and update the current tile
            transform.position = targetPosition;
        }
        else
        {
            Debug.LogError("Tile not found at position: " + targetPosition);
        }
    }
}