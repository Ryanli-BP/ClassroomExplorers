using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
public class PlayerMovement : MonoBehaviourPun
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
        if (isMoving || currentTile == null || diceroll == 0 || photonView.IsMine == false) return;
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
                    UIManager.Instance.OffDiceDisplay(); // off remaining step display when get into combat
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
                UIManager.Instance.OffDiceDisplay(); // off remaining step display when get into combat
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
                    int amount = 2;
                    otherPlayer.Heal(amount);
                    int receiverID = tilePlayerID;
                    int healerID = PlayerManager.Instance.CurrentPlayerID;
                    StartCoroutine(UIManager.Instance.DisplayHealing(healerID, receiverID, amount));
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
                    UIManager.Instance.OffDiceDisplay(); // off remaining step display when get into combat
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
            // display every step remain on player
            
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
            
                photonView.RPC("RPC_OFFdiceDisplay", RpcTarget.All); // off remaining step display when stop moving
                isMoving = false;
                
            }

            if (!isMoving) break;

            yield return StartCoroutine(HandleHomeTile(initialOnHome));
            if (!isMoving) { break; }
            initialOnHome = false;

            yield return StartCoroutine(HandlePaths(availableDirections));

            remainingSteps--;

            yield return StartCoroutine(UIManager.Instance.DisplayRemainingDiceSteps(remainingSteps));
            yield return new WaitForSeconds(0.05f);
            photonView.RPC("RPC_StepSync", RpcTarget.Others, remainingSteps);

            if (initialMove) //currently, initialMove is a condition for pvpencounter
            {
                initialMove = false;
            }
        }

        photonView.RPC("RPC_HandleMovementComplete", RpcTarget.All);
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
            photonView.RPC("RPC_MovementBroadcast", RpcTarget.Others, direction, PlayerManager.Instance.CurrentPlayerID);
            MovementAnimation movementAnimation = PlayerManager.Instance.GetCurrentPlayer().GetComponent<MovementAnimation>();
            yield return StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
        else
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
        }
    }
    // RPCS
    [PunRPC]
    private void RPC_MovementBroadcast(Direction direction, int playerID)
    {
        Player targetPlayer = PlayerManager.Instance.GetPlayerByID(playerID);
        if (targetPlayer == null)
        {
            Debug.LogError($"Player with ID {playerID} not found.");
            return;
        }

        PlayerMovement targetMovement = targetPlayer.GetComponent<PlayerMovement>();
        Tile currentTile = targetMovement.CurrentTile;

        if (currentTile == null)
        {
            Debug.LogError($"Current tile is null for player {playerID}.");
            return;
        }

        Tile nextTile = currentTile.GetConnectedTile(direction);
        if (nextTile == null)
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
            return;
        }

        currentTile.RemovePlayer(playerID);
        nextTile.AddPlayer(playerID);
        targetMovement.CurrentTile = nextTile;
        MovementAnimation movementAnimation = targetPlayer.GetComponent<MovementAnimation>();
        if (movementAnimation != null)
        {
            StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
    }
    [PunRPC]
    private void RPC_StepSync(int steps)
    {
        remainingSteps = steps;
        StartCoroutine(UIManager.Instance.DisplayRemainingDiceSteps(remainingSteps));
        
    }

    [PunRPC]
    private void RPC_OFFdiceDisplay()
    {
        UIManager.Instance.OffDiceDisplay();
    }

    [PunRPC]
    private void RPC_HandleMovementComplete()
    {
        this.isMoving = false;
        this.initialMove = true;
        OnMovementComplete?.Invoke();
    }
    

}