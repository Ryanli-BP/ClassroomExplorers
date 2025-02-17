using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
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

    [SerializeField] private List<GameObject> homeObjects;

    [SerializeField] private List<int> levelUpPoints = new List<int> { 10, 25, 50 };

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
        // If we're not connected, or not in a room, do nothing
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Photon not connected or not in room; skipping spawn.");
            return;
        }

        // If we already spawned our local player, skip
        // (You could track a bool, or see if PhotonNetwork.LocalPlayer.TagObject is set, etc.)
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Local player already spawned, skipping...");
            return;
        }

        // Retrieve selected player info (body/hat) from PlayerPrefs
        int selectedBodyColorIndex = PlayerPrefs.GetInt("SelectedBodyColorIndex", 0);
        int selectedHatIndex = PlayerPrefs.GetInt("SelectedHatIndex", 0);

        // Pack the customization data in instantiationData
        object[] instantiationData = new object[]
        {
            selectedBodyColorIndex,
            selectedHatIndex
        };

        // Actually spawn the local player's prefab
        GameObject playerGO = PhotonNetwork.Instantiate(
            "main player v2",   // Must match the prefab name in Resources
            transform.position, // or wherever you want initial spawn
            ARBoardPlacement.boardRotation * transform.rotation,
            0,
            instantiationData
        );

        // Optionally store a reference so we don’t spawn again
        PhotonNetwork.LocalPlayer.TagObject = playerGO;
        
        Debug.Log($"Spawned local player for ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber} with body={selectedBodyColorIndex} hat={selectedHatIndex}.");
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