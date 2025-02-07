using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-30)]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [SerializeField] private int _numOfPlayers = 2; // Backing field
    public int numOfPlayers
    {
        get => _numOfPlayers;
        set => _numOfPlayers = value;
    }

    [SerializeField] private List<Player> playerObjects;
    private List<Player> players = new List<Player>();

    [SerializeField] private List<GameObject> homeObjects;

    [SerializeField] private List<int> levelUpPoints = new List<int> { 5, 15, 40 };

    public int CurrentPlayerID { get; set; } = 1;

    public int CurrentHighLevel { get; set; } = 1; //needed to determine RoundPoints

    public List<Player> DeadPlayers { get; set; } = new List<Player>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    void Start()
    {
        InitialisePlayers();
        AssignPlayersToHomes();
        SpawnAllPlayersAtHome();
        GameInitializer.Instance.ConfirmManagerReady("PlayerManager");
    }

    private GameObject SpawnPlayer1()
    {
        // Retrieve selected character prefab index from PlayerPrefs
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0); // Default to 0 if not found

        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < playerObjects.Count)
        {
            GameObject player1Instance = Instantiate(playerObjects[selectedCharacterIndex].gameObject, Vector3.zero, Quaternion.identity);
            Debug.Log($"Player 1 spawned with prefab: {playerObjects[selectedCharacterIndex].name}");
            return player1Instance;
        }
        else
        {
            Debug.LogError("No valid character prefab found for Player 1!");
            return null;
        }
    }
    private void InitialisePlayers()
    {
        players.Clear();

        // Spawn Player 1 from the character selection prefab
        GameObject player1Instance = SpawnPlayer1();

        if (player1Instance != null)
        {
            Player player1 = player1Instance.GetComponent<Player>();
            player1.gameObject.SetActive(true);
            players.Add(player1);
            Debug.Log("Player 1 initialized and added.");
        }
        else
        {
            Debug.LogError("Player 1 prefab spawn failed!");
            return; // Exit early if Player 1 setup failed
        }

        // Instantiate and initialize other players from the prefab
        for (int i = 1; i < numOfPlayers; i++)
        {
            // Instantiate a new player instance from the prefab
            GameObject playerInstance = Instantiate(playerObjects[i % playerObjects.Count].gameObject);
            
            playerInstance.SetActive(true);
            Player playerComponent = playerInstance.GetComponent<Player>();
            
            players.Add(playerComponent);
            Debug.Log($"Player {playerComponent.getPlayerID()} activated.");
        }

        // Deactivate unused player objects
        for (int i = numOfPlayers; i < playerObjects.Count; i++)
        {
            playerObjects[i].gameObject.SetActive(false);
        }
    }


    public void AssignPlayersToHomes()
    {
        // Iterate over the players and assign them to a home
        for (int i = 0; i < players.Count; i++)
        {
            // Assuming home objects are assigned sequentially, you can modify this logic
            if (i < homeObjects.Count)
            {
                // Show the corresponding home object for the player
                homeObjects[i].SetActive(true);
            }
        }

        // Hide homes for unassigned players
        HideUnassignedHomes(homeObjects, players.Count);
    }

    public void HideUnassignedHomes(List<GameObject> homeObjects, int assignedPlayerCount)
    {
        // Iterate through all home objects
        for (int i = assignedPlayerCount; i < homeObjects.Count; i++)
        {
            // Hide any remaining homes that aren't assigned to players
            homeObjects[i].SetActive(false);
        }
    }

    public List<Player> GetPlayerList()
    {
        return players;
    }

    public Player GetPlayerByID(int playerID)
    {
        if (playerID > 0 && playerID <= players.Count)
        {
            Debug.Log($"CurrentID: {players[playerID-1]}");
            return players[playerID - 1];
        }
        else
        {
            Debug.LogError($"Invalid player ID: {playerID}. There are only {players.Count} players.");
            return null; // Or handle the invalid ID case as needed.
        }
    }

    public Player GetCurrentPlayer()
    {
        return players[CurrentPlayerID - 1];
    }

    public void GoNextPlayer()
    {
        // Use 0-based index for players, then adjust the player ID if needed.
        CurrentPlayerID = (CurrentPlayerID % players.Count) + 1;  // Wrap around 0-based index
        Debug.Log($"Current player ID is now {CurrentPlayerID}.");
    }

    public int GetNumOfPlayers()
    {
        return players.Count;
    }

    public int GetLevelUpPoints(int level)
    {
        return levelUpPoints[level - 1];
    }

    public void SpawnAllPlayersAtHome()
    {
        foreach (var player in players)
        {
            SpawnPlayerAtHome(player);
        }
    }

    public void SpawnPlayerAtHome(Player player)
    {
        Tile homeTile = TileManager.Instance.allTiles.Find(tile => tile.GetTileType() == TileType.Home && tile.GetHomePlayerID() == player.getPlayerID());

        if (homeTile != null)
        {
            Vector3 homePosition = homeTile.transform.position;
            homePosition.y += 0.2f * ARBoardPlacement.worldScale; // Adjust Y offset
            player.transform.position = homePosition;
            player.GetComponent<PlayerMovement>().CurrentTile = homeTile;
            Debug.Log($"Player {player.getPlayerID()} spawned at their home.");
        }
        else
        {
            Debug.LogError($"No home tile found for player {player.getPlayerID()}!");
        }
    }

    public void StartPlayerMovement(int diceTotal)
    {
        Player currentPlayer = GetCurrentPlayer();
        currentPlayer.GetComponent<PlayerMovement>().MovePlayer(diceTotal);
    }

    public void LevelUpPlayer()
        {
            Player currentPlayer = GetCurrentPlayer();
            int currentPoints = currentPlayer.Points;
            int currentLevel = currentPlayer.Level;

            if (currentLevel < levelUpPoints.Count + 1 && currentPoints >= levelUpPoints[currentLevel - 1])
            {
                Debug.Log($"points {currentPoints} level {currentLevel} leveling up as above {levelUpPoints[currentLevel - 1]}");
                currentPlayer.LevelUp();

                if (currentPlayer.Level > CurrentHighLevel)
                {
                    CurrentHighLevel = currentPlayer.Level;
                }

                // Check if the player has reached the last level
                if (currentPlayer.Level == levelUpPoints.Count + 1)
                {
                    GameManager.Instance.FinalLevelAchieved();
                    Debug.Log("Game End: Player has reached the maximum level.");
                }
            }
            else
            {
                Debug.Log($"Player {currentPlayer.getPlayerID()} does not have enough points to level up.");
            }
        }
}