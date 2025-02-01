using System.Collections.Generic;
using UnityEngine;
//iuhsfj    
public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    public GameObject tileContainer;
    public GameObject highlightOverlayPrefab; // Reference to the highlight overlay prefab
    public GameObject pathHighlightPrefab;
    public List<Tile> allTiles = new List<Tile>();
    private List<GameObject> activeHighlights = new List<GameObject>(); // Store active highlight overlays
    private List<GameObject> activePathHighlights = new List<GameObject>();

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
                GameManager.Instance.HandleQuizLand();
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
                    PlayerManager.Instance.GetCurrentPlayer().TeleportTo(destinationTile.transform.position, destinationTile);
                    tile.TilePlayerIDs.Remove(currentPlayerID);
                    destinationTile.TilePlayerIDs.Add(currentPlayerID);
                    //UIManager.Instance.DisplayTeleportEffect();
                }
                break;
        
            case TileType.Reroll:
                Debug.Log("Reroll tile - Player gets another turn");
                GameManager.Instance.HandleReroll();
                //UIManager.Instance.DisplayRerollEffect(); // Optional visual feedback
                break;
        }
    }

    public List<Tile> HighlightPossibleTiles(Tile startTile, int steps)
    {
        List<Tile> highlightedTiles = new List<Tile>();
        DFSHighlight(startTile, steps, highlightedTiles);
        return highlightedTiles;
    }

    private void DFSHighlight(Tile tile, int steps, List<Tile> highlightedTiles, List<Tile> currentPath = null)
    {
        // Initialize currentPath on first call
        if (currentPath == null)
            currentPath = new List<Tile>();
        
        currentPath.Add(tile);

        if (steps < 0)
        {
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        // Highlight the path tiles (except the last one)
        if (currentPath.Count > 1 && steps > 0)
        {
            HighlightPathTile(tile);
        }

        if (steps == 0)
        {
            HighlightTile(tile); // Highlight destination
            highlightedTiles.Add(tile);
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        List<Direction> directions = tile.GetAllAvailableDirections();
        foreach (Direction direction in directions)
        {
            Tile nextTile = tile.GetConnectedTile(direction);
            if (nextTile != null && !currentPath.Contains(nextTile))
            {
                DFSHighlight(nextTile, steps - 1, highlightedTiles, currentPath);
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
    }

    private void HighlightPathTile(Tile tile)
    {
        if (pathHighlightPrefab != null)
        {
            GameObject pathHighlight = Instantiate(pathHighlightPrefab, tile.transform.position + new Vector3(0, 0.005f, 0), Quaternion.Euler(0, 0, 0));
            pathHighlight.SetActive(true);
            activePathHighlights.Add(pathHighlight);
        }
        else
        {
            Debug.LogError("Path highlight prefab is not assigned!");
        }
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

        foreach (GameObject pathHighlight in activePathHighlights)
        {
            Destroy(pathHighlight);
        }
        activePathHighlights.Clear();
    }
}