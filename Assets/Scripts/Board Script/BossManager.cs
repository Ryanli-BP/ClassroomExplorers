using UnityEngine;
using System.Collections;

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

        Tile startTile = TileManager.Instance.allTiles[0];
        startTile.BossOnTile = true;
        if (startTile != null)
        {
            Vector3 spawnPosition = startTile.transform.position;
            spawnPosition.y += AboveTileOffset * BoardGenerator.BoardScale * ARBoardPlacement.worldScale;
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