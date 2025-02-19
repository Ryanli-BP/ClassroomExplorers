using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

[DefaultExecutionOrder(-30)]
public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance;
    public AvatarGenerator avatarGenerator;
    private PhotonView photonView;
    public const float AboveTileOffset = 0.5f; // Offset to place player above the tile

    [SerializeField] private Player playerPrefab;  // Main player prefab
    public GameObject[] bodyColors; // No need to serialize if not exposed to the Inspector
    public GameObject[] hats; // No need to serialize if not exposed to the Inspector

    private List<Player> players = new List<Player>();

    [SerializeField] private List<GameObject> homeObjects;

    [SerializeField] private List<int> PointsMilestone = new List<int> { 10, 25, 50, 100, 200 };

    public int CurrentPlayerID { get; set; } = 1;

    public int CurrentMilestone { get; set; } = 1; //needed to determine RoundPoints

    public List<Player> DeadPlayers { get; set; } = new List<Player>();

    private void Awake()
    {
        if (Instance == null){
            Instance = this;
            photonView = GetComponent<PhotonView>();
        }
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
        GameInitializer.Instance.ConfirmManagerReady("PlayerManager");
    }

    private void InitialisePlayers()
    {
    players.Clear();
    if (!PhotonNetwork.InRoom)
    {
        Debug.LogError("Not in a Photon Room! Cannot init players.");
        return;
    }

    // Ensure the client has the required properties before spawning
    if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("BodyColor") ||
        !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Hat"))
    {
        Debug.LogError("Missing player customization properties!");
        return;
    }

    //  Get customization settings from Photon
    int bodyColorIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["BodyColor"];
    int hatIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["Hat"];
    string nickname = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Nickname") 
                        ? (string)PhotonNetwork.LocalPlayer.CustomProperties["Nickname"] 
                        : "Player" + PhotonNetwork.LocalPlayer.ActorNumber;

    Quaternion playerRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(0, 0, 0);

    //  Only instantiate the local player
    GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, transform.position, playerRotation);
    Player playerScript = playerObject.GetComponent<Player>();

    playerScript.transform.localScale = playerScript.transform.localScale * ARBoardPlacement.worldScale;
    playerScript.SetPlayerID(PhotonNetwork.LocalPlayer.ActorNumber);

    //  Sync appearance across all clients using RPC
    playerScript.photonView.RPC("SetPlayerAppearance", RpcTarget.AllBuffered, bodyColorIndex, hatIndex);

    players.Add(playerScript);

    Debug.Log($"✅ Player {nickname} instantiated with BodyColor {bodyColorIndex}, Hat {hatIndex}.");

    // Adjust all players’ scale after everyone has spawned
    StartCoroutine(WaitForAllPlayersAndAdjustScale());
    }

    IEnumerator WaitForAllPlayersAndAdjustScale()
    {
        // Wait for all players to be in the game
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount == players.Count);

        foreach (var player in players)
        {
            player.transform.localScale = player.transform.localScale * ARBoardPlacement.worldScale;
            Debug.Log($"🔄 Adjusted scale for {player.name}");
        }
    }

    [PunRPC]
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



    /*public void AssignPlayersToHomes()
    {
        // Ensure we don't try to access more homes than exist
        int maxHomes = Mathf.Min(players.Count, homeObjects.Count);
        
        // Activate/deactivate all homes in one pass
        for (int i = 0; i < homeObjects.Count; i++)
        {
            homeObjects[i].SetActive(i < maxHomes);
        }
    }*/

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