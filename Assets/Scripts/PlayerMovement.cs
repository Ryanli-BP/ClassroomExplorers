using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Tile currentTile; // Assign the starting tile in the Inspector

    private bool isMoving = false;
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
        StartCoroutine(MoveStepByStep(diceroll));
    }

    private IEnumerator MoveStepByStep(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            List<Tile> neighbors = currentTile.GetNeighbors();
            if (neighbors.Count == 0)
            {
                Debug.LogError("No neighbors found! Player cannot move.");
                break;
            }

            // Randomly select a neighbor to move to (for now)
            Tile nextTile = neighbors[Random.Range(0, neighbors.Count)];
            Debug.Log($"Moving to tile: {nextTile.name}");

            // Adjust the player's position to the new tile's position with Y + 0.5 offset
            Vector3 targetPosition = nextTile.transform.position;
            targetPosition.y += 0.5f;
            transform.position = targetPosition;

            currentTile = nextTile;

            // Wait for a moment before moving to the next tile
            yield return new WaitForSeconds(0.5f);
        }

        isMoving = false;
    }
}
