using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    [SerializeField] private Boss bossObject;
    public const float AboveTileOffset = 0.5f;
    public Boss activeBoss { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (GameConfigManager.Instance.GetCurrentRules().haveBoss)
        {
            UIManager.Instance.ToggleBossUI(true);
            SpawnBoss();
        }
        else
        {
            UIManager.Instance.ToggleBossUI(false);
            bossObject.gameObject.SetActive(false);
        }
    }

    private void SpawnBoss()
    {
        bossObject.gameObject.SetActive(true);
        activeBoss = bossObject;
        StartCoroutine(SpawnBossAtStartTile());
    }

    private IEnumerator SpawnBossAtStartTile()
    {
        yield return new WaitUntil(() => GameInitializer.Instance.IsManagerReady("TileManager"));
        yield return new WaitUntil(() => GameInitializer.Instance.IsManagerReady("PlayerManager"));

        List<Tile> allTiles = TileManager.Instance.allTiles;
        List<Player> allPlayers = PlayerManager.Instance.GetPlayerList();
        
        // Find the tile that's farthest from all players
        Tile bestTile = null;
        float maxMinDistance = -1f;

        foreach (Tile candidateTile in allTiles)
        {
            // Skip tiles with special types if you want
            // if (candidateTile.GetTileType() == TileType.Home || candidateTile.GetTileType() == TileType.Special) continue;

            float minDistanceToPlayer = float.MaxValue;
            
            // Find the minimum distance from this tile to any player
            foreach (Player player in allPlayers)
            {
                Tile playerTile = player.GetComponent<PlayerMovement>().CurrentTile;
                if (playerTile != null)
                {
                    // Calculate distance between tiles (can use tile indices or world positions)
                    float distance = Vector3.Distance(candidateTile.transform.position, playerTile.transform.position);
                    minDistanceToPlayer = Mathf.Min(minDistanceToPlayer, distance);
                }
            }
            
            // If this tile is farther from all players than our previous best tile
            if (minDistanceToPlayer > maxMinDistance)
            {
                maxMinDistance = minDistanceToPlayer;
                bestTile = candidateTile;
            }
        }

        // If we couldn't find a suitable tile, fall back to the first tile
        Tile spawnTile = bestTile != null ? bestTile : allTiles[0];
        
        spawnTile.BossOnTile = true;
        Vector3 spawnPosition = spawnTile.transform.position;
        spawnPosition.y += AboveTileOffset * BoardGenerator.BoardScale * ARBoardPlacement.worldScale;
        activeBoss.transform.position = spawnPosition;
        activeBoss.Movement.CurrentTile = spawnTile;
        
        Debug.Log($"Boss spawned at tile (position: {spawnPosition}), distance from nearest player: {maxMinDistance}");
    }

    public void StartBossMovement(int diceTotal)
    {
        activeBoss.Movement.MoveBoss(diceTotal);
    }
}