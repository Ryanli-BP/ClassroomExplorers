using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    public GameObject tileContainer;
    public List<Tile> allTiles = new List<Tile>();
    private Dictionary<Vector3, Tile> tileDictionary = new Dictionary<Vector3, Tile>();

    void Awake()
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
        if (tileContainer != null)
        {
            allTiles.AddRange(tileContainer.GetComponentsInChildren<Tile>());
            foreach (var tile in allTiles)
            {
                tileDictionary[tile.transform.position] = tile;
            }
        }
        else
        {
            Debug.LogError("Tile container is not assigned! Please assign the tile container in the Inspector.");
        }
    }

    // This method finds a tile based on a position (rounded to the nearest grid position)
    public Tile GetTileAtPosition(Vector3 position)
    {
        tileDictionary.TryGetValue(position, out Tile tile);
        return tile;
    }

    private void LoadQuizScene()
    {
        // Save any necessary game state here (e.g., player position, current turn)
        // Load the Quiz scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Q&A Scene");
    }


    public void getTileAction(Tile tile)
    {
        switch (tile.GetTileType())
        {
            case TileType.Normal:
                Debug.Log("Normal tile");
                break;
            case TileType.GainPoint:
                Debug.Log("Gain point tile");
                PlayerManager.Instance.GetCurrentPlayer().AddPoints(Random.Range(3, 6) * PlayerManager.Instance.CurrentHighLevel);
                break; 
            case TileType.DropPoint:
                Debug.Log("Drop point tile");
                PlayerManager.Instance.GetCurrentPlayer().AddPoints(-1);
                break;
            case TileType.Home:
                Debug.Log("Home tile");
                PlayerManager.Instance.LevelUpPlayer();
                break;
            case TileType.Quiz:
                Debug.Log("Quiz tile");
                LoadQuizScene(); 
                break;
            case TileType.Buzz:
                Debug.Log("Buzz tile");
                // You can handle Buzz tile here later
                break;
            default:
                Debug.LogError("Unknown tile type");
                break;
        }
    }
}