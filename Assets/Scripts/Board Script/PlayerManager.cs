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

    [SerializeField] private Player playerPrefab;  // Main player prefab
    public GameObject[] bodyColors; // No need to serialize if not exposed to the Inspector
    public GameObject[] hats; // No need to serialize if not exposed to the Inspector

    private Dictionary<int, Player> players = new Dictionary<int, Player>();

    [SerializeField] private List<GameObject> homeObjects;

    [SerializeField] private List<int> levelUpPoints = new List<int> { 10, 25, 50 };

    public int CurrentPlayerID { get; set; } = 1;

    public int CurrentHighLevel { get; set; } = 1; //needed to determine RoundPoints

    public List<Player> DeadPlayers { get; set; } = new List<Player>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        int index = 0;
        foreach (var player in players.Values)
        {
            avatarGenerator.GenerateAvatar(player.gameObject, index);
            index++;
        }

        GameInitializer.Instance.ConfirmManagerReady("PlayerManager");
    }

    private void InitialisePlayers()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Photon not connected or not in room; skipping spawn.");
            return;
        }

        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Local player already spawned, skipping...");
            return;
        }

        int selectedBodyColorIndex = PlayerPrefs.GetInt("SelectedBodyColorIndex", 0);
        int selectedHatIndex = PlayerPrefs.GetInt("SelectedHatIndex", 0);

        object[] instantiationData = new object[]
        {
            selectedBodyColorIndex,
            selectedHatIndex
        };

        GameObject playerGO = PhotonNetwork.Instantiate(
            "main player v2",
            transform.position,
            ARBoardPlacement.boardRotation * transform.rotation,
            0,
            instantiationData
        );

        Player playerScript = playerGO.GetComponent<Player>();
        if (playerScript != null)
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            playerScript.SetPlayerID(actorNumber);  
            players[actorNumber] = playerScript;

            Debug.Log($"Added player {playerScript.getPlayerID()} to players list. Total players: {players.Count}");
        }
        else
        {
            Debug.LogError("Player component not found on instantiated object!");
        }

        PhotonNetwork.LocalPlayer.TagObject = playerGO;
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
        return new List<Player>(players.Values);
    }

    public Player GetPlayerByID(int playerID)
    {
        if (players.TryGetValue(playerID, out Player player))
        {
            return player;
        }
        else
        {
            Debug.LogError($"Invalid player ID: {playerID}. Total players: {players.Count}");
            return null;
        }
    }
    public Player GetCurrentPlayer()
    {
        if (players.Count == 0)
        {
            Debug.LogError("ERROR: Players list is empty!");
            return null;
        }

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (!players.ContainsKey(actorNumber))
        {
            Debug.LogError($"ERROR: Invalid player index {actorNumber}, players count = {players.Count}");
            return null;
        }

        return players[actorNumber];
    }



    public void GoNextPlayer()
    {
        List<int> playerIDs = new List<int>(players.Keys);
        if (playerIDs.Count == 0)
        {
            Debug.LogError("No players in the game!");
            return;
        }

        int currentIndex = playerIDs.IndexOf(PhotonNetwork.LocalPlayer.ActorNumber);
        int nextIndex = (currentIndex + 1) % playerIDs.Count;
        CurrentPlayerID = playerIDs[nextIndex];

        Debug.Log($" Next turn: Player {CurrentPlayerID}.");
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
        foreach (var player in players.Values)
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
            photonView.RPC("SyncPlayersList", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.Log($"Player {player.getPlayerID()} spawned at their home.");

        }
        else
        {
            Debug.LogError($"No home tile found for player {player.getPlayerID()}!");
        }
    }
    [PunRPC]
    void SyncPlayersList(int actorNumber)
    {
        if (!players.ContainsKey(actorNumber))
        {
            Player player = FindPlayerByActorNumber(actorNumber);
            if (player != null)
            {
                players[actorNumber] = player;
                Debug.Log($"[SYNC] Added player {player.getPlayerID()} to the dictionary. Total players: {players.Count}");
            }
        }
    }
    private Player FindPlayerByActorNumber(int actorNumber)
    {
        players.TryGetValue(actorNumber, out Player player);
        return player;
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

            if (currentLevel >= Player.MAX_LEVEL)
            {
                Debug.Log($"Player {currentPlayer.getPlayerID()} is already at the maximum level.");
                return;
            }

            if (currentLevel < Player.MAX_LEVEL && currentPoints >= levelUpPoints[currentLevel - 1])
            {
                Debug.Log($"points {currentPoints} level {currentLevel} leveling up as above {levelUpPoints[currentLevel - 1]}");
                currentPlayer.LevelUp();

                if (currentPlayer.Level > CurrentHighLevel)
                {
                    CurrentHighLevel = currentPlayer.Level;
                }

                // Check if the player has reached the last level
                if (currentPlayer.Level == Player.MAX_LEVEL && GameConfigManager.Instance.CurrentMode == GameMode.FFA)
                {
                    GameManager.Instance.WinGameConditionAchieved();
                    Debug.Log("Game End: Player has reached the maximum level.");
                }
            }
            else
            {
                Debug.Log($"Player {currentPlayer.getPlayerID()} does not have enough points to level up.");
            }
        }
}