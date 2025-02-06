using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    [SerializeField] private Boss bossPrefab;
    private Boss activeBoss;

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
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        activeBoss = Instantiate(bossPrefab, 
            Vector3.zero, 
            ARBoardPlacement.boardRotation);
        activeBoss.transform.localScale *= ARBoardPlacement.worldScale;
        SpawnBossAtStartTile();
    }

    private void SpawnBossAtStartTile()
    {
        Tile startTile = TileManager.Instance.allTiles[0]; // Or pick specific tile
        if (startTile != null)
        {
            Vector3 spawnPosition = startTile.transform.position;
            spawnPosition.y += 0.2f * ARBoardPlacement.worldScale;
            activeBoss.transform.position = spawnPosition;
            activeBoss.CurrentTile = startTile;
            Debug.Log("Boss spawned at start tile");
        }
        else
        {
            Debug.LogError("No start tile found for boss!");
        }
    }

    public void HandleBossTurn()
    {
        Debug.Log("Starting Boss Turn");
        StartCoroutine(ExecuteBossTurn());
    }
    private IEnumerator ExecuteBossTurn()
    {
        // Display "Boss's Turn" message
        UIManager.Instance.DisplayBossTurn();
        
        // Wait for a short duration
        yield return new WaitForSeconds(2f);
        
        // Proceed to next round
        GameManager.Instance.HandleBossTurnEnd();
    }
}