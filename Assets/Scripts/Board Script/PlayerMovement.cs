using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Tile currentTile; // Assigned dynamically during spawn
    [SerializeField] private int playerID;
    [SerializeField] private Direction lastDirection; // Tracks the last direction the player came from

    private bool isMoving = false;
    private int remainingSteps = 0;

    public void Initialize(Tile homeTile, int id)
    {
        playerID = id;
        currentTile = homeTile;
        transform.position = homeTile.transform.position + Vector3.up * 0.5f; // Offset to visually sit above the tile
    }

    public void MovePlayer(int diceroll)
    {
        if (isMoving || currentTile == null)
            return;

        remainingSteps = diceroll;
        StartCoroutine(MoveStepByStep());
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private IEnumerator MoveStepByStep()
    {
        isMoving = true;

        while (remainingSteps > 0)
        {
            List<Direction> availableDirections = currentTile.GetAllAvailableDirections(lastDirection);

            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }

            // Handle crossroads or move in the only available direction
            if (availableDirections.Count > 1)
            {
                yield return StartCoroutine(WaitForPlayerInput(availableDirections));
            }
            else
            {
                MoveToNextTile(availableDirections[0]);
            }

            remainingSteps--;
            yield return new WaitForSeconds(0.15f);
        }

        isMoving = false;
    }

    private void MoveToNextTile(Direction direction)
    {
        lastDirection = direction;

        Vector3 targetPosition = currentTile.transform.position;

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

        currentTile = TileManager.Instance.GetTileAtPosition(targetPosition);

        if (currentTile != null)
        {
            transform.position = targetPosition;
        }
        else
        {
            Debug.LogError("Tile not found at position: " + targetPosition);
        }
    }

    private IEnumerator WaitForPlayerInput(List<Direction> availableDirections)
    {
        Direction? chosenDirection = null;

        UIManager.Instance.ShowDirectionChoices(availableDirections, (direction) =>
        {
            chosenDirection = direction;
        });

        yield return new WaitUntil(() => chosenDirection != null);

        MoveToNextTile(chosenDirection.Value);
    }
}
