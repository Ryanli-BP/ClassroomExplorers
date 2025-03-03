using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public GameObject anvilPrefab;

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

    IEnumerator Start()
    {
        yield return new WaitUntil(() => BoardGenerator.BoardGenFinished == true);
        
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

    private int getBonus(Player player)
    {   
        if (player.PlayerBuffs.TriplePoints)
            return 3;
        if (player.PlayerBuffs.DoublePoints)
            return 2;
        return 1;
    }

    public IEnumerator getPlayerTileAction(Tile tile)
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
                float milestoneMultiplier = 1f + (0.5f * (PlayerManager.Instance.CurrentMilestone - 1));
                int basePoints = UnityEngine.Random.Range(1, 6);
                int finalPoints = Mathf.RoundToInt(basePoints * milestoneMultiplier);

                int multiplier = getBonus(currentPlayer);
                int pointsGained = basePoints * multiplier;

                if (multiplier > 1)
                {
                    UIManager.Instance.SetBonusUIValue(multiplier);
                }

                yield return StartCoroutine(UIManager.Instance.DisplayPointChange(basePoints)); //base points because the UI deal with bonus
                UIManager.Instance.DisplayGainStarAnimation(currentPlayerID);

                StartCoroutine(currentPlayer.AddPoints(pointsGained));
                break;

            case TileType.DropPoint:
                Debug.Log("Drop point tile");
                    float dropMilestoneMultiplier = 1f + (0.5f * (PlayerManager.Instance.CurrentMilestone - 1));
                    int basePointsLost = UnityEngine.Random.Range(1, 6);
                    int finalPointsLost = -Mathf.RoundToInt(basePointsLost * dropMilestoneMultiplier);
                
                StartCoroutine(UIManager.Instance.DisplayPointChange(finalPointsLost));
                UIManager.Instance.DisplayLoseStarAnimation();

                StartCoroutine(currentPlayer.AddPoints(finalPointsLost));
                break;
            
            case TileType.Quiz:
                Debug.Log("Quiz tile");
                QuizManager.Instance.StartNewQuiz();
                yield return new WaitUntil(() => QuizManager.Instance.OnQuizComplete);
                break;

            case TileType.Home:
                Debug.Log("Home tile");
                PlayerManager.Instance.HomeTileAction();

                currentPlayer.Heal(2);

                break;

            case TileType.Portal:
                Debug.Log("Portal tile");
                Tile destinationTile = GetRandomPortalTile(tile);
                if (destinationTile != null)
                {
                    currentPlayer.TeleportTo(destinationTile.transform.position, destinationTile);
                    tile.TilePlayerIDs.Remove(currentPlayerID);
                    destinationTile.TilePlayerIDs.Add(currentPlayerID);
                    yield return new WaitForSeconds(0.5f);
                    //UIManager.Instance.DisplayTeleportEffect();
                }
                break;
        
            case TileType.Reroll:
                Debug.Log("Reroll tile - Player gets another turn");
                GameManager.Instance.HandleReroll();
                //UIManager.Instance.DisplayRerollEffect(); // Optional visual feedback
                break;
            
            case TileType.Trap:
                Debug.Log("Trap tile");
                yield return StartCoroutine(PlayAnvilAnimation(currentPlayer.transform.position));
                int damage = UnityEngine.Random.Range(1, 3);
                currentPlayer.LoseHealth(damage);
                yield return StartCoroutine(UIManager.Instance.DisplayDamageNumber(currentPlayer.transform.position, damage));
                break;
        }

            OnTileActionComplete = true;
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

    public Direction GetDirectionTowardsPlayers(Tile startTile, List<Direction> availableDirections)
    {
        Dictionary<Tile, Direction> initialDirections = new Dictionary<Tile, Direction>();
        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        
        // Initialize with all available first moves
        foreach (Direction dir in availableDirections)
        {
            Tile nextTile = startTile.GetConnectedTile(dir);
            if (nextTile != null)
            {
                queue.Enqueue(nextTile);
                initialDirections[nextTile] = dir;
            }
        }

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();
            
            if (visited.Contains(currentTile))
                continue;

            visited.Add(currentTile);

            // Found players on this tile - since this is BFS, this is guaranteed to be shortest path
            if (currentTile.TilePlayerIDs.Count > 0)
            {
                Direction foundDirection = initialDirections[currentTile];
                return foundDirection;
            }

            // Explore neighbors
            foreach (Direction dir in currentTile.GetAllAvailableDirections())
            {
                Tile nextTile = currentTile.GetConnectedTile(dir);
                if (nextTile != null && !visited.Contains(nextTile))
                {
                    queue.Enqueue(nextTile);
                    if (!initialDirections.ContainsKey(nextTile))
                    {
                        initialDirections[nextTile] = initialDirections[currentTile];
                    }
                }
            }
        }

        // If no path to players found, return random direction
        Direction random = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
        Debug.Log($"No path to players found. Choosing random direction: {random}");
        return random;
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
            float heightOffset = 0.5f * BoardGenerator.BoardScale * ARBoardPlacement.worldScale;
            Vector3 tileCenter = tile.transform.position;
            Vector3 overlayPosition = new Vector3(
                tileCenter.x,
                tileCenter.y + heightOffset + 0.002f,
                tileCenter.z
            );

            Quaternion highlightRotation = ARBoardPlacement.boardRotation * Quaternion.Euler(90, 0, 0);
            GameObject highlight = Instantiate(highlightOverlayPrefab, overlayPosition, highlightRotation);
                
            // Use the prefab's original scale multiplied by board scale
            highlight.transform.localScale = ARBoardPlacement.worldScale * BoardGenerator.BoardScale * highlightOverlayPrefab.transform.localScale;
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
                float heightOffset = 0.5f * BoardGenerator.BoardScale * ARBoardPlacement.worldScale;
                Vector3 tileCenter = tile.transform.position;
                Vector3 overlayPosition = new Vector3(
                    tileCenter.x,
                    tileCenter.y + heightOffset + 0.001f, // Small additional offset to prevent z-fighting
                    tileCenter.z
                );

                GameObject darkOverlay = Instantiate(darkOverlayPrefab, overlayPosition, ARBoardPlacement.boardRotation);
                darkOverlay.transform.localScale = ARBoardPlacement.worldScale * BoardGenerator.BoardScale * darkOverlayPrefab.transform.localScale;
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

    private IEnumerator PlayAnvilAnimation(Vector3 playerPosition)
    {
        // Spawn anvil 3 units above the player
        Vector3 spawnPosition = playerPosition + Vector3.up * 4f * ARBoardPlacement.worldScale;
        GameObject anvil = Instantiate(anvilPrefab, spawnPosition, ARBoardPlacement.boardRotation * Quaternion.identity );
        anvil.transform.localScale = anvil.transform.localScale * ARBoardPlacement.worldScale;
        
        // Animation parameters
        float dropDuration = 0.5f;
        float elapsedTime = 0f;
        Vector3 targetPosition = playerPosition + Vector3.up * ARBoardPlacement.worldScale; // Slightly above player
        
        // Drop animation
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dropDuration;
            
            // Add acceleration using square function
            float verticalPosition = Mathf.Lerp(spawnPosition.y, targetPosition.y, progress * progress);
            anvil.transform.position = new Vector3(spawnPosition.x, verticalPosition, spawnPosition.z);
            
            yield return null;
        }
        
        // Impact effect
        anvil.transform.position = targetPosition;
        
        // Optional: Add camera shake or impact effect here
        
        // Wait briefly before destroying
        yield return new WaitForSeconds(0.5f);
        Destroy(anvil);
    }
}