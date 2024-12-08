using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Tile currentTile; // Assign the starting tile in the Inspector

    private bool isMoving = false;
    private int remainingSteps = 0; // Tracks steps left to move
    private Direction lastDirection; // To track the direction the player came from

    private void OnEnable()
    {
        // Subscribe to the OnDiceTotal event
        DiceDisplay.OnDiceTotal += MovePlayer;
    }

    private void OnDisable()
    {
        // Unsubscribe when the object is disabled to avoid memory leaks
        DiceDisplay.OnDiceTotal -= MovePlayer;
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

        while (remainingSteps > 0)
        {
            // Get valid directions based on the last direction
            List<Direction> availableDirections = currentTile.GetAllAvailableDirections(lastDirection);

            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }

            // If at a crossroads, stop and wait for player input
            if (availableDirections.Count > 1)
            {
                Debug.Log("At a crossroad! Waiting for player to choose a direction...");
                yield return StartCoroutine(WaitForPlayerInput(availableDirections));
            }
            else
            {
                // Move in the only available direction
                Direction nextDirection = availableDirections[0];
                Debug.Log($"Moving in the direction: {nextDirection}");
                MoveToNextTile(nextDirection);
            }

            remainingSteps--;
            yield return new WaitForSeconds(0.25f); // Optional delay for smoother movement
        }

        isMoving = false;
    }

    private void MoveToNextTile(Direction direction)
    {
        // Update last direction based on the current movement direction
        lastDirection = direction;

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

        // Find the tile at the new position (grid-based system)
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


    private IEnumerator WaitForPlayerInput(List<Direction> availableDirections)
    {
        Direction? chosenDirection = null; // Nullable Direction, starts as null

        // Display available directions (e.g., show UI buttons for each direction)
        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) => {
            chosenDirection = direction;
        });

        // Wait until the player makes a choice (chosenDirection is not null)
        yield return new WaitUntil(() => chosenDirection != null);

        Debug.Log($"Player chose to move in the direction: {chosenDirection}");

        // Move to the next tile in the chosen direction
        MoveToNextTile(chosenDirection.Value);
    }
}
