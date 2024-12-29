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
            // Get valid directions based on the last direction         
            List<Direction> availableDirections = currentTile.GetAllAvailableDirections(_lastDirection);
            
            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }
            // Prompt the player if they reach their home tile
            if (currentTile.GetTileType() == TileType.Home  && currentTile.GetPlayerID() == PlayerManager.Instance.getCurrentPlayerID() && !initialOnHome)
            {
                Debug.Log("Reached home tile. Prompting player to choose.");
                yield return StartCoroutine(HandleHomeTile());
            }

            initialOnHome = false;

            if (!isMoving) { yield break; } //if player chooses to stop

            // If at a crossroads, stop and wait for player input
            if (availableDirections.Count > 1)
            {
                Debug.Log("At a crossroad! Waiting for player to choose a direction...");
                yield return StartCoroutine(HandleDirections(availableDirections));
            }
            else
            {
                // Move in the only available direction
                Direction nextDirection = availableDirections[0];
                //Debug.Log($"Moving in the direction: {nextDirection}");
                MoveToNextTile(nextDirection);
            }

            remainingSteps--;
            yield return new WaitForSeconds(0.15f); // Optional delay for smoother movement
        }

        isMoving = false;
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

    private IEnumerator HandleDirections(List<Direction> availableDirections)
    {
        Direction? chosenDirection = null;

        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) => {
            chosenDirection = direction;
        });

        // Wait until the player makes a choice 
        yield return new WaitUntil(() => chosenDirection != null);

        Debug.Log($"Player chose to move in the direction: {chosenDirection}");

        // Move to the next tile in the chosen direction
        MoveToNextTile(chosenDirection.Value);
    }

    private IEnumerator HandleHomeTile()
    {
        bool? playerChoice = null;

        UIManager.Instance.ShowHomeTilePrompt((choice) => {
            playerChoice = choice;
        });

        yield return new WaitUntil(() => playerChoice != null);

        if (playerChoice == true)
        {
            Debug.Log("Player chose to stay on the home tile.");
            isMoving = false;
            OnMovementComplete?.Invoke();
        }
        else
        {
            Debug.Log("Player chose to continue moving.");
        }
    }
}

