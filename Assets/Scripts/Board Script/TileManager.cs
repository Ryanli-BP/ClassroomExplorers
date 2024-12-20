using System.Collections.Generic;
using UnityEngine;

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
}