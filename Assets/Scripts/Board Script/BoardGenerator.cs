using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class BoardData
{
    public int width;
    public int height;
    public TileData[] tiles;
}

[Serializable]
public class TileData
{
    public int x;
    public int z;  // Changed from y to z
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

    private const string BOARD_LAYOUT_PATH = "boardLayout";
    private const float TILE_SPACING = 1f;
    private const float Y_POSITION = -0.5f;  // Default Y position for all tiles

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
            // Use X and Z for horizontal positioning, fixed Y for height
            Vector3 position = new Vector3(
                tileData.x * TILE_SPACING, 
                Y_POSITION, 
                tileData.z * TILE_SPACING
            );
            
            GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, tileContainer.transform);
            Tile tile = tileObj.GetComponent<Tile>();
            
            // Store tile reference using X and Z coordinates
            Vector2Int coord = new Vector2Int(tileData.x, tileData.z);
            tileMap[coord] = tile;
            tileDataMap[coord] = tileData;

            SetTileType(tile, (TileType)(tileData.type - 1));

            if (tileData.homePlayerID > 0)
            {
                SerializedObject serializedTile = new SerializedObject(tile);
                SerializedProperty homePlayerIDProperty = serializedTile.FindProperty("HomeplayerID");
                homePlayerIDProperty.intValue = tileData.homePlayerID;
                serializedTile.ApplyModifiedProperties();
            }
        }

        // Second pass: Set up connections
        foreach (var kvp in tileDataMap)
        {
            Vector2Int currentPos = kvp.Key;
            TileData tileData = kvp.Value;
            Tile currentTile = tileMap[currentPos];

            SerializedObject serializedTile = new SerializedObject(currentTile);

            foreach (string connection in tileData.connections)
            {
                Direction dir = directionMap[connection.ToLower()];
                Vector2Int targetPos = GetTargetPosition(currentPos, dir);

                if (tileMap.TryGetValue(targetPos, out Tile targetTile))
                {
                    string propertyName = $"{connection.ToLower()}Tile";
                    SerializedProperty connectionProperty = serializedTile.FindProperty(propertyName);
                    connectionProperty.objectReferenceValue = targetTile;
                }
            }

            serializedTile.ApplyModifiedProperties();
        }
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
        SerializedObject serializedTile = new SerializedObject(tile);
        SerializedProperty tileTypeProperty = serializedTile.FindProperty("tileType");
        tileTypeProperty.enumValueIndex = (int)type;
        serializedTile.ApplyModifiedProperties();

        // Apply material based on tile type
        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = GetMaterialForTileType(type);
        }
    }
}