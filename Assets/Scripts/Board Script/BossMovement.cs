using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossMovement : MonoBehaviour
{
    private Tile currentTile;
    private bool isMoving = false;
    private bool initialMove = true;
    private int remainingSteps = 0;
    private MovementAnimation movementAnimation;
    public event Action OnMovementComplete;

    public Tile CurrentTile
    {
        get { return currentTile; }
        set { currentTile = value; }
    }

    private IEnumerator HandleBossCombat()
    {
        if (currentTile.TilePlayerIDs.Count > 0)
        {
            foreach (int tilePlayerID in currentTile.TilePlayerIDs)
            {
                Entity Player = PlayerManager.Instance.GetPlayerByID(tilePlayerID);
                // Skip if the player on tile is dead
                if (Player.Status != Status.Alive)
                {
                    continue;
                }

                Debug.Log($"Boss encountered Player {tilePlayerID}. Initiating combat.");
                GameManager.Instance.OnCombatTriggered();
                
                // Pass boss ID as the opponent (assuming it's stored in BossManager)
                yield return StartCoroutine(CombatManager.Instance.HandleFight(
                    BossManager.Instance.activeBoss, 
                    Player));
                
                GameManager.Instance.IsResumingMovement = false;
                remainingSteps = 0; // Stop movement after combat
                UIManager.Instance.OffDiceDisplay(); //stop UI dice display
                
                if (BossManager.Instance.activeBoss.Status == Status.Dead)
                {
                    isMoving = false;
                    yield break; // Exit if boss dies
                }
            }
        }
    }

    public void MoveBoss(int diceRoll)
    {
        if (isMoving || currentTile == null)
            return;

        Debug.Log($"Boss rolled: {diceRoll} steps");
        remainingSteps = diceRoll;
        StartCoroutine(MoveStepByStep());
    }

    private IEnumerator MoveStepByStep()
    {
        isMoving = true;
        initialMove = true;

        while (remainingSteps >= 0)
        {
            Debug.Log($"Remaining steps: {remainingSteps}");
            // display every step remain on boss
            yield return StartCoroutine(UIManager.Instance.DisplayRemainingDiceSteps(remainingSteps));
            
            
            // Handle combat before movement
            if (!initialMove)
            {
                yield return StartCoroutine(HandleBossCombat());
            }

            //Finishes handling all movement actions on final tile
            if (remainingSteps == 0)
            {
                UIManager.Instance.OffDiceDisplay();//stop display dice number
                isMoving = false;
            }

            if (!isMoving) break;

            List<Direction> availableDirections = currentTile.GetAllAvailableDirections();
            
            if (availableDirections.Count == 0)
            {
                Debug.LogError("No valid directions found! Boss cannot move.");
                break;
            }

            // Randomly select from available directions
            Direction nextDirection = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
            yield return StartCoroutine(MoveToNextTileCoroutine(nextDirection));

            remainingSteps--;
            yield return new WaitForSeconds(0.5f);

            if (initialMove) //currently, initialMove is a condition for pvpencounter
            {
                initialMove = false;
            }
        }

        isMoving = false;
        OnMovementComplete?.Invoke();
    }

    private IEnumerator MoveToNextTileCoroutine(Direction direction)
    {
        Tile nextTile = currentTile.GetConnectedTile(direction);

        if (nextTile != null)
        {

            if (currentTile != null)
            {
                currentTile.BossOnTile = false;
            }

            nextTile.BossOnTile = true;
            currentTile = nextTile;
            movementAnimation = BossManager.Instance.activeBoss.GetComponent<MovementAnimation>();
            yield return StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
        else
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
        }
    }
}