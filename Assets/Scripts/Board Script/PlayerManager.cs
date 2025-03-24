using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[DefaultExecutionOrder(-30)]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public AvatarGenerator avatarGenerator;

    public const float AboveTileOffset = 0.5f; // Offset to place player above the tile

    [SerializeField] private Player playerPrefab;  // Main player prefab
    public GameObject[] bodyColors; // No need to serialize if not exposed to the Inspector
    public GameObject[] hats; // No need to serialize if not exposed to the Inspector

    private List<Player> players = new List<Player>();

    [SerializeField] private GameObject homeObject;
    public List<Texture2D> flagTextures; // color same order as body color

    [SerializeField] private List<int> PointsMilestone = new List<int> { 10, 25, 50, 100, 200 };

    public int CurrentPlayerID { get; set; } = 1;

    public int CurrentMilestone { get; set; } = 1; //needed to determine RoundPoints

    public List<Player> DeadPlayers { get; set; } = new List<Player>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameConfigManager.Instance.IsFetchComplete);
        yield return new WaitUntil(() => GameInitializer.Instance.IsManagerReady("TileManager"));


        InitialisePlayers();
        //AssignPlayersToHomes();
        SpawnAllPlayersAtHome();
        // Generate avatars after players list is fully populated
        for (int i = 0; i < players.Count; i++)
        {
            avatarGenerator.GenerateAvatar(players[i].gameObject, i);
        }

        //Assign home object to home tile
        SpawnAllHomesAtHome();

        GameInitializer.Instance.ConfirmManagerReady("PlayerManager");
    }

    private void InitialisePlayers()
    {
        players.Clear();

        // Retrieve selected player information from the previous scene
        int selectedPlayerIndex = PlayerPrefs.GetInt("SelectedBodyColorIndex", 0); // Default to 0 if not set
        int selectedHatIndex = PlayerPrefs.GetInt("SelectedHatIndex", 0);

        for (int i = 0; i < GameConfigManager.Instance.numOfPlayers; i++)
        {
            // Instantiate the player prefab
            Quaternion playerRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(0, 0, 0);
            Player playerObject = Instantiate(playerPrefab, 
                                transform.position, 
                                playerRotation);
            playerObject.transform.localScale = playerObject.transform.localScale * ARBoardPlacement.worldScale;

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
        Transform hatParent = playerObject.transform.Find("hats");

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
            // Assuming hats are child objects under "hats"
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

    public int GetMilestonePoints(int level)
    {
        return PointsMilestone[level];
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
            homePosition.y += AboveTileOffset * BoardGenerator.BoardScale * ARBoardPlacement.worldScale; // Adjust Y offset
            player.transform.position = homePosition;
            player.GetComponent<PlayerMovement>().CurrentTile = homeTile;
            Debug.Log($"Player {player.getPlayerID()} spawned at their home.");
        }
        else
        {
            Debug.LogError($"No home tile found for player {player.getPlayerID()}!");
        }
    }

     // spawn player's home object one by one
    public void SpawnAllHomesAtHome()
    {
        int index = 0;
        foreach (var player in players)
        {
            SpawnHomeObjectAtHome(player, index);
            index += 1;
        }
    }

    public void SpawnHomeObjectAtHome(Player player, int index)
    {
        Tile homeTile = TileManager.Instance.allTiles.Find(tile => tile.GetTileType() == TileType.Home && tile.GetHomePlayerID() == player.getPlayerID());

        if (homeTile != null)
        {
            // Instantiate the home object prefab
            Quaternion homeRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(0, 0, 0);
            GameObject home = Instantiate(homeObject, 
                                transform.position, 
                                homeRotation);
            home.transform.localScale = home.transform.localScale * ARBoardPlacement.worldScale;

            //change color of flag according to body color
            Transform flagChild = home.transform.Find("flag2_low");
            if (flagChild == null)
            {
                Debug.LogError("Child object 'flag2_low' not found in instantiated flag!");
                return;
            }
            Renderer flagRenderer = flagChild.GetComponent<Renderer>();
            //set the texture using the index and the list of textures
            //for now using playerID, as multiplayer is not done yet
            if (flagRenderer != null && index >= 0 && index < flagTextures.Count)
            {
                // Apply the selected texture
                flagRenderer.material.SetTexture("_BaseMap", flagTextures[index]); // URP Shader
                // OR for Standard Shader
                // flagRenderer.material.mainTexture = flagTextures[textureIndex];

                Debug.Log($"Flag spawned with texture index {index}");
            }
            else
            {
                Debug.LogError("Renderer not found on 'flag2_low' or invalid texture index!");
            }

            //assign the home object to home tile
            Vector3 homePosition = homeTile.transform.position;
            homePosition.y += AboveTileOffset * BoardGenerator.BoardScale * ARBoardPlacement.worldScale; // Adjust Y offset
            home.transform.position = homePosition;
            Debug.Log($"Player {player.getPlayerID()} home spawned at their home.");
        }
        else
        {
            Debug.LogError($"No home cannot spawn for player {player.getPlayerID()}!");
        }
    }


    public void StartPlayerMovement(int diceTotal)
    {
        Player currentPlayer = GetCurrentPlayer();
        currentPlayer.GetComponent<PlayerMovement>().MovePlayer(diceTotal);
    }

    public void HomeTileAction()
    {
        if (GameConfigManager.Instance.CurrentMode == GameMode.COOP)
        {
            LevelUpPlayer();
        }
        else
        {
            PlayerEarnTrophy();
        }
    }

    public void PlayerEarnTrophy()
    {
        Player currentPlayer = GetCurrentPlayer();
        int currentPoints = currentPlayer.Points;
        int currentTrophy = currentPlayer.TrophyCount;

        if (currentTrophy < Player.MAX_TROPHY && currentPoints >= PointsMilestone[currentTrophy])
        {
            Debug.Log($"points {currentPoints} Trophy {currentTrophy} getting trophy as above {PointsMilestone[currentTrophy]}");
            currentPlayer.EarnTrophy();

            if (currentPlayer.TrophyCount > CurrentMilestone)
            {
                CurrentMilestone = currentPlayer.TrophyCount;
            }
        }
        else
        {
            Debug.Log($"Player {currentPlayer.getPlayerID()} does not have enough points to earn trophy.");
        }

        // Check if the player has already earned max trophies
        if (currentPlayer.TrophyCount == Player.MAX_TROPHY && GameConfigManager.Instance.CurrentMode != GameMode.COOP)
        {
            GameManager.Instance.WinGameConditionAchieved();
            Debug.Log("Game End: Player has collected all trophies.");
        }
    }

    public void LevelUpPlayer()
    {
        Player currentPlayer = GetCurrentPlayer();
        int currentPoints = currentPlayer.Points;
        int currentLevel = currentPlayer.Level;

        if (currentLevel >= Player.MAX_LEVEL)
        {
            Debug.Log($"Player {currentPlayer.getPlayerID()} is already at the maximum level.");
            return;
        }

        if (currentLevel < Player.MAX_LEVEL && currentPoints >= PointsMilestone[currentLevel])
        {
            Debug.Log($"points {currentPoints} level {currentLevel} leveling up as above {PointsMilestone[currentLevel]}");
            currentPlayer.LevelUp();

            if (currentPlayer.Level > CurrentMilestone)
            {
                CurrentMilestone = currentPlayer.Level;
            }
        }
        else
        {
            Debug.Log($"Player {currentPlayer.getPlayerID()} does not have enough points to level up.");
        }

    }
}