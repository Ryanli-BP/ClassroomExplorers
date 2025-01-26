using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Tile currentTile; // Assign the starting tile in the Inspector
    private Direction _lastDirection; // To track the direction the player came from
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

    private IEnumerator HandlePvPEncounter()
    {
        if (currentTile.TilePlayerID != 0 && 
            currentTile.TilePlayerID != PlayerManager.Instance.CurrentPlayerID && 
            PlayerManager.Instance.GetPlayerByID(currentTile.TilePlayerID).Status == Status.Alive)
        {
            Debug.Log($"Player {currentTile.TilePlayerID} is on this tile.");
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
                yield return StartCoroutine(CombatManager.Instance.HandleFight(currentTile.TilePlayerID, PlayerManager.Instance.CurrentPlayerID));
                GameManager.Instance.IsResumingMovement = false;
                if (PlayerManager.Instance.GetCurrentPlayer().Status == Status.Dead)
                {
                    isMoving = false;
                }
            }
            else
            {
                Debug.Log("Player chose to continue moving.");
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

    private IEnumerator HandleCrossroads(List<Direction> availableDirections)
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

        while (remainingSteps >= 0)
        {
            if (initialMove)
            {
                currentTile.TilePlayerID = 0;
                initialMove = false;
            }

            List<Direction> availableDirections = currentTile.GetAllAvailableDirections();
            
            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Player cannot move.");
                break;
            }
            
            yield return StartCoroutine(HandlePvPEncounter());

            if (PlayerManager.Instance.GetCurrentPlayer().Status == Status.Dead)
            {
                Debug.Log("Player is dead. Cannot move.");
                break;
            }

            if (remainingSteps == 0)
            {
                isMoving = false;
                break;
            }

            yield return StartCoroutine(HandleHomeTile(initialOnHome));
            if (!isMoving) { break; }

            initialOnHome = false;

            yield return StartCoroutine(HandleCrossroads(availableDirections));

            remainingSteps--;
            yield return new WaitForSeconds(0.05f);
        }

        isMoving = false;
        initialMove = true;
        OnMovementComplete?.Invoke();
    }


    private IEnumerator MoveToNextTileCoroutine(Direction direction)
    {
        Debug.Log("MoveToNextTile called with direction: " + direction);
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

        Debug.Log("Starting MoveToNextTileCoroutine to target position: " + targetPosition);
        yield return StartCoroutine(MoveToNextTileCoroutine(targetPosition));
    }

    private IEnumerator MoveToNextTileCoroutine(Vector3 targetPosition)
    {
        Debug.Log("MoveToNextTileCoroutine started");

        currentTile = TileManager.Instance.GetTileAtPosition(targetPosition);

        if (currentTile != null)
        {
            PlayerMovementAnimation movementAnimation = PlayerManager.Instance.GetCurrentPlayer().GetComponent<PlayerMovementAnimation>();
            yield return StartCoroutine(movementAnimation.HopTo(targetPosition));
        }
        else
        {
            Debug.LogError("Tile not found at position: " + targetPosition);
        }

        Debug.Log("MoveToNextTileCoroutine ended");
    }
}