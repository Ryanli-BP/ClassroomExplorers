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
        if (photonView.IsMine)
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
            Debug.Log($"Remaining steps: {remainingSteps}");
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
            MovementAnimation movementAnimation = PlayerManager.Instance.GetCurrentPlayer().GetComponent<MovementAnimation>();
            photonView.RPC("RPC_SyncPlayerMovement", RpcTarget.Others, currentTile.transform.position, nextTile.transform.position, PlayerManager.Instance.CurrentPlayerID);
            yield return StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
        else
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
        }
    }

    //RPCS
    [PunRPC]
    private void RPC_SyncPlayerMovement(Vector3 currentTilePos, Vector3 nextTilePos, int playerID)
    {
        // Get tiles by position
        Tile currentTileRemote = TileManager.Instance.GetTileAtPosition(currentTilePos);
        Tile nextTileRemote = TileManager.Instance.GetTileAtPosition(nextTilePos);

        if (currentTileRemote != null)
        {
            currentTileRemote.RemovePlayer(playerID);
        }

        if (nextTileRemote != null)
        {
            nextTileRemote.AddPlayer(playerID);
            // Update the current tile for this player's movement component
            Player player = PlayerManager.Instance.GetPlayerByID(playerID);
            if (player != null)
            {
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.CurrentTile = nextTileRemote;
                }
            }
            MovementAnimation movementAnimation = PlayerManager.Instance.GetPlayerByID(playerID).GetComponent<MovementAnimation>();
            StartCoroutine(movementAnimation.HopTo(nextTileRemote.transform.position));
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
        GameManager.Instance.HandleMovementComplete();
      
        
}
    

}