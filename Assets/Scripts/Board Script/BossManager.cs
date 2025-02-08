using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    [SerializeField] private Boss bossObject;
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
            SpawnBoss();
        }
        else
        {
            bossObject.gameObject.SetActive(false);
        }
    }

    private void SpawnBoss()
    {
        bossObject.gameObject.SetActive(true);
        activeBoss = bossObject;
        SpawnBossAtStartTile();
    }

    private void SpawnBossAtStartTile()
    {
        Tile startTile = TileManager.Instance.allTiles[0];
        startTile.BossOnTile = true;
        if (startTile != null)
        {
            Vector3 spawnPosition = startTile.transform.position;
            spawnPosition.y += 0.2f * ARBoardPlacement.worldScale;
            activeBoss.transform.position = spawnPosition;
            activeBoss.Movement.CurrentTile = startTile;
            Debug.Log("Boss spawned at start tile");
        }
        else
        {
            Debug.LogError("No start tile found for boss!");
        }
    }

    public void StartBossMovement(int diceTotal)
    {
        activeBoss.Movement.MoveBoss(diceTotal);
    }
}