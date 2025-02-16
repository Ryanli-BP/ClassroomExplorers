using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class ArrowData
{
    public float x;
    public float z;
    public string rotation; // Rotation in direction
}

[Serializable]
public class BoardData
{
    public int width;
    public int height;
    public TileData[] tiles;
    public ArrowData[] arrows;
}

[Serializable]
public class TileData
{
    public int x;
    public int z; 
    public int type;
    public string[] connections;
    public int homePlayerID;
}

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject tileContainer;

    [Header("Tile Materials")]
    public Material normalTileMaterial;
    public Material gainPointTileMaterial;
    public Material dropPointTileMaterial;
    public Material homeTileMaterial;
    public Material quizTileMaterial;
    public Material portalTileMaterial;
    public Material rerollTileMaterial;

    [Header("Arrow Prefab")]
    public GameObject arrowPrefab;

    private const string BOARD_LAYOUT_PATH = "boardLayout";
    private const float TILE_SPACING = 1f;
    private const float Y_POSITION = -0.5f;  // Default Y position for all tiles
    private const float ARROW_Y_POSITION = 0f;  // Default Y position for all arrows
    private const float ARROW_DEFAULT_X_ROTATION = 90f;
    private const float ARROW_DEFAULT_Z_ROTATION = 0f;
    private const float ARROW_DEFAULT_SCALE = 15f;

    public const float BoardScale = 1.8f;

    public static bool BoardGenFinished { get; private set; } = false;

    private Dictionary<string, float> directionToAngle = new Dictionary<string, float>
    {
        {"north", 90f},
        {"east", 180f},
        {"south", 270f},
        {"west", 0f}
    };

    private Dictionary<string, Direction> directionMap = new Dictionary<string, Direction>
    {
        {"north", Direction.North},
        {"east", Direction.East},
        {"south", Direction.South},
        {"west", Direction.West}
    };

    void Start()
    {
        GenerateBoardFromJSON();
    }

    void GenerateBoardFromJSON()
    {
         TextAsset jsonFile = Resources.Load<TextAsset>(BOARD_LAYOUT_PATH);
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load board layout JSON file!");
            return;
        }

        BoardData boardData = JsonUtility.FromJson<BoardData>(jsonFile.text);
        Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();
        Dictionary<Vector2Int, TileData> tileDataMap = new Dictionary<Vector2Int, TileData>();

        // First pass: Create all tiles
        foreach (TileData tileData in boardData.tiles)
        {
            Vector3 position = new Vector3(
                tileData.x * TILE_SPACING, 
                Y_POSITION, 
                tileData.z * TILE_SPACING
            );
            
            GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, tileContainer.transform);
            tileObj.transform.localScale = Vector3.one * 1;
            Tile tile = tileObj.GetComponent<Tile>();
            
            Vector2Int coord = new Vector2Int(tileData.x, tileData.z);
            tileMap[coord] = tile;
            tileDataMap[coord] = tileData;

            SetTileType(tile, (TileType)(tileData.type - 1));

            if (tileData.homePlayerID > 0)
            {
                tile.HomeplayerID = tileData.homePlayerID;
            }
        }

        //Create all arrows
        if (boardData.arrows != null)
        {
            foreach (ArrowData arrowData in boardData.arrows)
            {
                Vector3 position = new Vector3(
                    arrowData.x * TILE_SPACING,
                    ARROW_Y_POSITION,
                    arrowData.z * TILE_SPACING
                );

                // Get rotation angle from direction string
                float yRotation = directionToAngle[arrowData.rotation.ToLower()];
                
                Quaternion rotation = Quaternion.Euler(
                    ARROW_DEFAULT_X_ROTATION,
                    yRotation,
                    ARROW_DEFAULT_Z_ROTATION
                );

                GameObject arrowObj = Instantiate(arrowPrefab, 
                    position, 
                    rotation, 
                    tileContainer.transform);

                // Apply both the default scale and the AR world scale
                arrowObj.transform.localScale = Vector3.one * ARROW_DEFAULT_SCALE * 1; //put ARBoardPlacement.worldScale back after testing
            }
        }

        // Second pass: Set up connections
        foreach (var kvp in tileDataMap)
        {
            Vector2Int currentPos = kvp.Key;
            TileData tileData = kvp.Value;
            Tile currentTile = tileMap[currentPos];

            foreach (string connection in tileData.connections)
            {
                Direction dir = directionMap[connection.ToLower()];
                Vector2Int targetPos = GetTargetPosition(currentPos, dir);

                if (tileMap.TryGetValue(targetPos, out Tile targetTile))
                {
                    switch (connection.ToLower())
                    {
                        case "north":
                            currentTile.northTile = targetTile;
                            break;
                        case "south":
                            currentTile.southTile = targetTile;
                            break;
                        case "east":
                            currentTile.eastTile = targetTile;
                            break;
                        case "west":
                            currentTile.westTile = targetTile;
                            break;
                    }
                }
            }
        }

        tileContainer.transform.localScale = Vector3.one * BoardScale * ARBoardPlacement.worldScale;
        BoardGenFinished = true;
    }

    private Vector2Int GetTargetPosition(Vector2Int current, Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return new Vector2Int(current.x, current.y + 1);
            case Direction.South: return new Vector2Int(current.x, current.y - 1);
            case Direction.East: return new Vector2Int(current.x + 1, current.y);
            case Direction.West: return new Vector2Int(current.x - 1, current.y);
            default: return current;
        }
    }

    private Material GetMaterialForTileType(TileType type)
    {
        return type switch
        {
            TileType.Normal => normalTileMaterial,
            TileType.GainPoint => gainPointTileMaterial,
            TileType.DropPoint => dropPointTileMaterial,
            TileType.Home => homeTileMaterial,
            TileType.Quiz => quizTileMaterial,
            TileType.Portal => portalTileMaterial,
            TileType.Reroll => rerollTileMaterial,
            _ => normalTileMaterial
        };
    }

    private void SetTileType(Tile tile, TileType type)
    {
        tile.tileType = type;

        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = GetMaterialForTileType(type);
        }
    }
}