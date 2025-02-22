using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
[DefaultExecutionOrder(-30)]
public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance;
    public AvatarGenerator avatarGenerator;

    public const float AboveTileOffset = 0.5f; // Offset to place player above the tile
    [SerializeField] private PhotonView photonView;

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

        foreach (var netPlayer in PhotonNetwork.PlayerList)
        {
            if (!netPlayer.CustomProperties.ContainsKey("BodyColor") ||
                !netPlayer.CustomProperties.ContainsKey("Hat"))
            {
                Debug.LogError($"Player {netPlayer.NickName} is missing customization properties!");
                continue;
            }
            int bodyColorIndex = (int)netPlayer.CustomProperties["BodyColor"];
            int hatIndex = (int)netPlayer.CustomProperties["Hat"];
            string nickname = netPlayer.CustomProperties.ContainsKey("Nickname")
                                ? (string)netPlayer.CustomProperties["Nickname"]
                                : "Player" + netPlayer.ActorNumber;
            // Instantiate the player prefab
            Quaternion playerRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(0, 0, 0);
            GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, transform.position, playerRotation);
            Player playerScript = playerObject.GetComponent<Player>();
            playerScript.transform.localScale *= ARBoardPlacement.worldScale;
            

            playerScript.SetPlayerID(netPlayer.ActorNumber);
            // Set the player appearance before adding to the list
            playerScript.gameObject.SetActive(true);
            PhotonView pv = playerObject.GetComponent<PhotonView>();
            if(pv != null)
            {
                pv.RPC("SetPlayerAppearance", RpcTarget.AllBuffered, bodyColorIndex, hatIndex);
            }
            else
            {
                Debug.LogError("PhotonView missing on instantiated player!");
            }
            players.Add(playerScript);

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
        CurrentPlayerID = (CurrentPlayerID % players.Count) + 1;  
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
        photonView.RPC("RPCStartPlayerMovement", RpcTarget.All, CurrentPlayerID, diceTotal);

    }
    [PunRPC]
    private void RPCStartPlayerMovement(int playerID, int diceTotal)
    {
        Player player = GetPlayerByID(playerID);
        if (player != null)
        {
            player.GetComponent<PlayerMovement>().MovePlayer(diceTotal);
        }
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