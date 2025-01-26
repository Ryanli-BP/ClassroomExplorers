using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    public GameObject tileContainer;
    public GameObject highlightOverlayPrefab; // Reference to the highlight overlay prefab
    public List<Tile> allTiles = new List<Tile>();
    private Dictionary<Vector3, Tile> tileDictionary = new Dictionary<Vector3, Tile>();
    private List<GameObject> activeHighlights = new List<GameObject>(); // Store active highlight overlays

    private List<Tile> portalTiles = new List<Tile>(); // Add this field

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
                if (tile.GetTileType() == TileType.Portal)
                {
                    portalTiles.Add(tile);
                }
            }
        }
        else
        {
            Debug.LogError("Tile container is not assigned! Please assign the tile container in the Inspector.");
        }
    }

    public Tile GetTileAtPosition(Vector3 position)
    {
        tileDictionary.TryGetValue(position, out Tile tile);
        return tile;
    }

    private Tile GetRandomPortalTile(Tile currentTile)
    {
        if (portalTiles.Count <= 1) return null;
        
        List<Tile> availablePortals = new List<Tile>(portalTiles);
        availablePortals.Remove(currentTile);
        if (availablePortals.Count == 0) return null;
        
        int randomIndex = Random.Range(0, availablePortals.Count);
        return availablePortals[randomIndex];
    }

    public void getTileAction(Tile tile)
    {
        var currentPlayerID = PlayerManager.Instance.CurrentPlayerID;
        tile.TilePlayerID = currentPlayerID;
        
        switch (tile.GetTileType())
        {
            case TileType.Normal:
                Debug.Log("Normal tile");
                break;

            case TileType.GainPoint:
                Debug.Log("Gain point tile");
                int pointsGained = Random.Range(1, 6) * PlayerManager.Instance.CurrentHighLevel;
                PlayerManager.Instance.GetCurrentPlayer().AddPoints(pointsGained);
                UIManager.Instance.DisplayPointChange(pointsGained);
                UIManager.Instance.DisplayGainStarAnimation(currentPlayerID);
                break; 

            case TileType.DropPoint:
                Debug.Log("Drop point tile");
                int pointsLost = -(Random.Range(1, 6) * PlayerManager.Instance.CurrentHighLevel);
                PlayerManager.Instance.GetCurrentPlayer().AddPoints(pointsLost);
                UIManager.Instance.DisplayPointChange(pointsLost);
                UIManager.Instance.DisplayLoseStarAnimation();
                break;
            
            case TileType.Quiz:
                Debug.Log("Quiz tile");
                QuizManager.Instance.StartNewQuiz();
                break;

            case TileType.Home:
                Debug.Log("Home tile");
                PlayerManager.Instance.LevelUpPlayer();
                PlayerManager.Instance.GetCurrentPlayer().HealPLayer(1);
                break;

            case TileType.Portal:
                Debug.Log("Portal tile");
                Tile destinationTile = GetRandomPortalTile(tile);
                if (destinationTile != null)
                {
                    PlayerManager.Instance.GetCurrentPlayer().TeleportTo(destinationTile.transform.position);
                }
                break;
        }
    }

    public List<Tile> HighlightPossibleTiles(Tile startTile, int steps)
    {
        List<Tile> highlightedTiles = new List<Tile>();
        DFSHighlight(startTile, steps, highlightedTiles);
        return highlightedTiles;
    }

    private void DFSHighlight(Tile tile, int steps, List<Tile> highlightedTiles)
    {
        if (steps < 0)
            return;

        if (steps == 0)
        {
            HighlightTile(tile); // Highlight only the tiles where steps reach zero
            highlightedTiles.Add(tile);
            return;
        }

        List<Direction> directions = tile.GetAllAvailableDirections(Direction.None);
        foreach (Direction direction in directions)
        {
            Tile nextTile = GetNextTile(tile, direction);
            if (nextTile != null)
            {
                DFSHighlight(nextTile, steps - 1, highlightedTiles);
            }
        }
    }

    private Tile GetNextTile(Tile tile, Direction direction)
    {
        Vector3 targetPosition = tile.transform.position;

        switch (direction)
        {
            case Direction.North:
                targetPosition += new Vector3(0, 0, 1);
                break;
            case Direction.East:
                targetPosition += new Vector3(1, 0, 0);
                break;
            case Direction.South:
                targetPosition += new Vector3(0, 0, -1);
                break;
            case Direction.West:
                targetPosition += new Vector3(-1, 0, 0);
                break;
        }

        return GetTileAtPosition(targetPosition);
    }

    private void HighlightTile(Tile tile)
    {
        if (highlightOverlayPrefab != null)
        {
            Debug.Log("Highlighting tile at position: " + tile.transform.position);
            GameObject highlight = Instantiate(highlightOverlayPrefab, tile.transform.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(90, 0, 0));
            highlight.SetActive(true);
            activeHighlights.Add(highlight);
        }
        else
        {
            Debug.LogError("Highlight overlay prefab is not assigned!");
        }
    }

    public void ClearHighlightedTiles()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            Destroy(highlight);
        }
        activeHighlights.Clear();
    }
}