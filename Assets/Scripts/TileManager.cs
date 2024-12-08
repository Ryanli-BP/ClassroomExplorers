using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    // Singleton instance
    public static TileManager Instance { get; private set; }

    // Container object that holds all the tiles (Drag this container into the inspector)
    public Transform tileContainer;

    // A list to store all tiles in the game
    public List<Tile> allTiles = new List<Tile>();

    private void Awake()
    {
        // Ensure there is only one instance of TileManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Populate the allTiles list by getting all Tile components in the tileContainer
        if (tileContainer != null)
        {
            allTiles.AddRange(tileContainer.GetComponentsInChildren<Tile>());
        }
        else
        {
            Debug.LogError("Tile container is not assigned! Please assign the tile container in the Inspector.");
        }
    }

    // This method finds a tile based on a position (rounded to the nearest grid position)
    public Tile GetTileAtPosition(Vector3 position)
    {
        foreach (var tile in allTiles)
        {
            if (tile.transform.position == position)
            {
                return tile;
            }
        }

        return null; // Return null if no tile is found at the given position
    }
}
