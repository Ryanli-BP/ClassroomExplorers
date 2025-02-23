using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
     [SerializeField] private PhotonView photonView;
    private Tile currentTile; 
    private Direction _lastDirection; // To track the direction the player came from
    private bool isMoving = false;
    private bool initialMove; //One time flag for avoiding some actions intially for movement
    private int remainingSteps = 0;
    public event Action OnMovementComplete;
    private bool canFightPlayers;
    private bool canHealPlayers;
    private bool haveBoss;

    private void Awake()
    {
        // Get PhotonView if not assigned in inspector
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
    }
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
        if (isMoving || currentTile == null)
            return;

        Debug.Log($"Rolled: {diceroll} steps");
        remainingSteps = diceroll;
        StartCoroutine(MoveStepByStep());
    }    
    private bool IsMyTurn()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Debug.LogError("PhotonView is null and couldn't be found!");
                return false;
            }
        }

        Player player = GetComponent<Player>();
        if (player == null) return false;

        return photonView.IsMine && 
               PlayerManager.Instance.CurrentPlayerID == player.getPlayerID();
    }

    [PunRPC]
    private void RPCSetDirection(int directionInt)
    {
        Direction direction = (Direction)directionInt;
        _lastDirection = direction;

        if (currentTile == null)
        {
            // Recover tile if it's null
            currentTile = TileManager.Instance.GetTileAtPosition(transform.position);
            
            if (currentTile == null)
            {
                Debug.LogError($"Failed to recover current tile for player {PlayerManager.Instance.CurrentPlayerID}");
                return;
            }
            Debug.Log($"Successfully recovered tile for player {PlayerManager.Instance.CurrentPlayerID}");
        }

        StartCoroutine(MoveToNextTileCoroutine(direction));
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
            
            // Only show stop option to active player
            if (IsMyTurn())
            {
                yield return StartCoroutine(PromptManager.Instance.HandleHomeTile((choice) => {
                    if (choice)
                    {
                        photonView.RPC("RPCStopMovement", RpcTarget.All);
                    }
                }));
            }
            else
            {
                // Wait for active player's choice
                while (isMoving)
                {
                    yield return null;
                }
            }
        }
    }

    [PunRPC]
    private void RPCStopMovement()
    {
        UIManager.Instance.OffDiceDisplay();
        Debug.Log("Player chose to stay on the home tile.");
        isMoving = false;
    }

    private IEnumerator HandlePaths(List<Direction> availableDirections)
    {
        if (availableDirections.Count > 1)
        {
            Debug.Log("At a crossroad! Waiting for player to choose a direction...");
            List<Tile> highlightedTiles = TileManager.Instance.HighlightPossibleTiles(currentTile, remainingSteps);
            Direction? selectedDirection = null;
            
            // Only show direction UI to the active player
            if (IsMyTurn())
            {
                yield return StartCoroutine(PromptManager.Instance.HandleDirections(availableDirections, (direction) => {
                    if (photonView.IsMine)
                    {
                        photonView.RPC("RPCSetDirection", RpcTarget.All, (int)direction);
                    }
                }));
            }
            else
            {
                // Wait for the active player's choice
                while (_lastDirection == Direction.None)
                {
                    yield return null;
                }
            }

            TileManager.Instance.ClearHighlightedTiles();
        }
        else
        {
            Direction nextDirection = availableDirections[0];
            if (IsMyTurn())
            {
                photonView.RPC("RPCSetDirection", RpcTarget.All, (int)nextDirection);
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
                UIManager.Instance.OffDiceDisplay(); // off remaining step display when stop moving
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

            if (initialMove) //currently, initialMove is a condition for pvpencounter
            {
                initialMove = false;
            }
        }

        isMoving = false;
        initialMove = true;

            OnMovementComplete?.Invoke();      
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
            yield return StartCoroutine(movementAnimation.HopTo(nextTile.transform.position));
        }
        else
        {
            Debug.LogError($"No connected tile found in direction: {direction}");
        }
    }
}