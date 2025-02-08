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

    [SerializeField] private Player playerPrefab;  // Main player prefab
    public GameObject[] bodyColors; // No need to serialize if not exposed to the Inspector
    public GameObject[] hats; // No need to serialize if not exposed to the Inspector

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

    private void InitialisePlayers()
    {
        players.Clear();

        // Retrieve selected player information from the previous scene
        int selectedPlayerIndex = PlayerPrefs.GetInt("SelectedBodyColorIndex", 0); // Default to 0 if not set
        int selectedHatIndex = PlayerPrefs.GetInt("SelectedHatIndex", 0);

        for (int i = 0; i < numOfPlayers; i++)
        {
            // Instantiate the player prefab
            Player playerObject = Instantiate(playerPrefab, transform);
            playerObject.SetPlayerID(i + 1);
            // Set the player appearance before adding to the list
            int bodyColorIndex = i == 0 ? selectedPlayerIndex : i; // Use selected color for Player 1, default for others
            int hatIndex = i == 0 ? selectedHatIndex : i; // Use selected hat for Player 1, default for others

            playerObject.gameObject.SetActive(true);
            
            SetPlayerAppearance(playerObject, bodyColorIndex, hatIndex);
            
            // Add the instantiated GameObject (with Player script) to the players list
            players.Add(playerObject);

            // Assuming playerPrefab already contains a Player component that will be automatically added
            Debug.Log($"Player {i + 1} instantiated and appearance set.");
        }
        playerPrefab.SetPlayerID(-1);
    }
    
    public void SetPlayerAppearance(Player playerObject, int selectedBodyIndex, int selectedHatIndex)
    {
        // Find the body parent object (e.g., "Mesh Object/Bone_Body") for this specific player
        Transform bodyParent = playerObject.transform.Find("Mesh Object/Bone_Body");
        Transform hatParent = playerObject.transform.Find("Mesh Object/hats");

        if (bodyParent != null && bodyColors.Length > 0)
        {
            // Assuming bodyColors are child objects under "Bone_Body"
            for (int i = 0; i < bodyColors.Length; i++)
            {
                Transform bodyColorTransform = bodyParent.GetChild(i); // Get the child transform for each body color
                if (i == selectedBodyIndex)
                {
                    bodyColorTransform.gameObject.SetActive(true); // Activate the selected body color
                }
                else
                {
                    bodyColorTransform.gameObject.SetActive(false); // Deactivate the other body colors
                }
            }
        }

        if (hatParent != null && hats.Length > 0)
        {
            // Assuming hats are child objects under "Bone_Hat"
            for (int i = 0; i < hats.Length; i++)
            {
                Transform hatTransform = hatParent.GetChild(i); // Get the child transform for each hat
                if (i == selectedHatIndex)
                {
                    hatTransform.gameObject.SetActive(true); // Activate the selected hat
                }
                else
                {
                    hatTransform.gameObject.SetActive(false); // Deactivate the other hats
                }
            }
        }

        Debug.Log($"Player appearance set for {playerObject.name}: Body Index {selectedBodyIndex}, Hat Index {selectedHatIndex}");
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