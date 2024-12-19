using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public List<Player> players;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // For now, spawn only the first player
        if (players.Count > 0)
        {
            SpawnPlayerAtHome(players[0]);
        }
    }

    public void SpawnPlayerAtHome(Player player)
    {
        Tile homeTile = TileManager.Instance.allTiles.Find(tile => tile.isHome && tile.playerID == player.playerID);

        if (homeTile != null)
        {
            Vector3 homePosition = homeTile.transform.position;
            homePosition.y += 0.5f; // Adjust Y offset
            player.transform.position = homePosition;
            player.GetComponent<PlayerMovement>().SetCurrentTile(homeTile);
            Debug.Log($"Player {player.playerID} spawned at their home.");
        }
        else
        {
            Debug.LogError($"No home tile found for player {player.playerID}!");
        }
    }
}