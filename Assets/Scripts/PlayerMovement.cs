using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Tile currentTile; // Assign the starting tile in the Inspector

    private bool isMoving = false;
    private int remainingSteps = 0; // Tracks steps left to move

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
            List<Tile> neighbors = currentTile.GetNeighbors();

            if (neighbors.Count == 0)
            {
                Debug.LogError("No neighbors found! Player cannot move.");
                break;
            }

            // If at a crossroads, stop and wait for player input
            if (neighbors.Count > 1)
            {
                Debug.Log("At a crossroad! Waiting for player to choose a direction...");
                yield return StartCoroutine(WaitForPlayerInput(neighbors));
            }
            else
            {
                // Move to the only available neighbor
                Tile nextTile = neighbors[0];
                Debug.Log($"Moving to tile: {nextTile.name}");
                MoveToTile(nextTile);
            }

            remainingSteps--;
            yield return new WaitForSeconds(0.5f); // Optional delay for smoother movement
        }

        isMoving = false;
    }

    private void MoveToTile(Tile tile)
    {
        Vector3 targetPosition = tile.transform.position;
        targetPosition.y += 0.5f; // Adjust Y offset
        transform.position = targetPosition;

        currentTile = tile;
    }

    private IEnumerator WaitForPlayerInput(List<Tile> neighbors)
    {
        Tile chosenTile = null;

        // Display available directions (e.g., show UI buttons for each neighbor)
        // Example: Assume we display directions through a UI Manager
        UIManager.Instance.ShowDirectionChoices(neighbors, (tile) => {
            chosenTile = tile;
        });

        // Wait until the player makes a choice
        yield return new WaitUntil(() => chosenTile != null);

        Debug.Log($"Player chose to move to: {chosenTile.name}");
        MoveToTile(chosenTile);
    }
}
