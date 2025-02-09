using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    public GameObject tileContainer;
    public GameObject darkOverlayPrefab;
    private List<GameObject> activeDarkOverlays = new List<GameObject>();
    public GameObject highlightOverlayPrefab; // Reference to the highlight overlay prefab
    public List<Tile> allTiles = new List<Tile>();
    private List<GameObject> activeHighlights = new List<GameObject>(); // Store active highlight overlays
    public bool OnTileActionComplete { get; set; }

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

        GameInitializer.Instance.ConfirmManagerReady("TileManager");
    }

    private Tile GetRandomPortalTile(Tile currentTile)
    {
        if (portalTiles.Count <= 1) return null;
        
        List<Tile> availablePortals = new List<Tile>(portalTiles);
        availablePortals.Remove(currentTile);
        if (availablePortals.Count == 0) return null;
        
        int randomIndex = UnityEngine.Random.Range(0, availablePortals.Count);
        return availablePortals[randomIndex];
    }

    public void getPlayerTileAction(Tile tile)
    {
        var currentPlayerID = PlayerManager.Instance.CurrentPlayerID;
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
        
        switch (tile.GetTileType())
        {
            case TileType.Normal:
                Debug.Log("Normal tile");
                break;

            case TileType.GainPoint:
                Debug.Log("Gain point tile");
                int pointsGained = UnityEngine.Random.Range(1, 6) * PlayerManager.Instance.CurrentHighLevel;
                currentPlayer.AddPoints(pointsGained);
                UIManager.Instance.DisplayPointChange(pointsGained);
                UIManager.Instance.DisplayGainStarAnimation(currentPlayerID);
                break; 

            case TileType.DropPoint:
                Debug.Log("Drop point tile");
                int pointsLost = -(UnityEngine.Random.Range(1, 6) * PlayerManager.Instance.CurrentHighLevel);
                currentPlayer.AddPoints(pointsLost);
                UIManager.Instance.DisplayPointChange(pointsLost);
                UIManager.Instance.DisplayLoseStarAnimation();
                break;
            
            case TileType.Quiz:
                Debug.Log("Quiz tile");
                GameManager.Instance.HandleQuizStart();
                break;

            case TileType.Home:
                Debug.Log("Home tile");
                PlayerManager.Instance.LevelUpPlayer();
                if(currentPlayerID == tile.GetHomePlayerID())
                {
                    currentPlayer.Heal(1);
                }
                break;

            case TileType.Portal:
                Debug.Log("Portal tile");
                Tile destinationTile = GetRandomPortalTile(tile);
                if (destinationTile != null)
                {
                    currentPlayer.TeleportTo(destinationTile.transform.position, destinationTile);
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

        if (tile.GetTileType() != TileType.Quiz)
        {
            OnTileActionComplete = true;
        }
    }

    public void getBossTileAction(Tile tile)
    {
        Boss currentBoss = BossManager.Instance.activeBoss;
        
        switch (tile.GetTileType())
        {
            case TileType.Normal:
                Debug.Log("Boss on Normal tile");
                break;

            case TileType.Portal:
                Debug.Log("Portal tile");
                Tile destinationTile = GetRandomPortalTile(tile);
                if (destinationTile != null)
                {
                    currentBoss.TeleportTo(destinationTile.transform.position, destinationTile);
                    tile.BossOnTile = false;
                    destinationTile.BossOnTile = true;
                    //UIManager.Instance.DisplayTeleportEffect();
                }
                break;
        
            case TileType.Reroll:
                Debug.Log("Reroll tile - Player gets another turn");
                GameManager.Instance.HandleReroll();
                //UIManager.Instance.DisplayRerollEffect(); // Optional visual feedback
                break;
        }

        OnTileActionComplete = true;
    }

    public List<Tile> HighlightPossibleTiles(Tile startTile, int steps)
    {
        List<Tile> highlightedTiles = new List<Tile>();
        HashSet<Tile> pathAndDestinationTiles = new HashSet<Tile>();
        
        // First find all valid paths and destinations
        DFSHighlight(startTile, steps, highlightedTiles, null, pathAndDestinationTiles);
        
        // Then darken tiles not in paths or destinations
        DarkenNonHighlightedTiles(pathAndDestinationTiles);
        
        return highlightedTiles;
    }

    private void DFSHighlight(Tile tile, int steps, List<Tile> highlightedTiles, List<Tile> currentPath = null, HashSet<Tile> allValidTiles = null)
    {
        if (currentPath == null)
            currentPath = new List<Tile>();
        if (allValidTiles == null)
            allValidTiles = new HashSet<Tile>();
        
        currentPath.Add(tile);
        allValidTiles.Add(tile); // Add to valid tiles but don't highlight path

        if (steps < 0)
        {
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        if (steps == 0)
        {
            HighlightTile(tile); // Only highlight destination
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
                DFSHighlight(nextTile, steps - 1, highlightedTiles, currentPath, allValidTiles);
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
    }

    private void HighlightTile(Tile tile)
    {
        if (highlightOverlayPrefab != null)
        {
            
            Quaternion highlightRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(90, 0, 0);
            GameObject highlight = Instantiate(highlightOverlayPrefab, 
            tile.transform.position + new Vector3(0, 0.005f, 0), 
            highlightRotation);
                
            highlight.transform.localScale = highlight.transform.localScale * ARBoardPlacement.worldScale;
            highlight.SetActive(true);
            activeHighlights.Add(highlight);
        }
    }

    private void DarkenNonHighlightedTiles(HashSet<Tile> validTiles)
    {
        if (darkOverlayPrefab == null)
        {
            Debug.LogError("Dark overlay prefab is not assigned!");
            return;
        }

        foreach (Tile tile in allTiles)
        {
            if (!validTiles.Contains(tile))
            {
                GameObject darkOverlay = Instantiate(darkOverlayPrefab, 
                tile.transform.position + new Vector3(0, 0.003f, 0), 
                ARBoardPlacement.boardRotation);
                darkOverlay.transform.localScale = darkOverlay.transform.localScale * ARBoardPlacement.worldScale;
                darkOverlay.SetActive(true);
                activeDarkOverlays.Add(darkOverlay);
            }
        }
    }

    public void ClearHighlightedTiles()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            Destroy(highlight);
        }
        activeHighlights.Clear();

        foreach (GameObject darkOverlay in activeDarkOverlays)
        {
            Destroy(darkOverlay);
        }
        activeDarkOverlays.Clear();
    }
}