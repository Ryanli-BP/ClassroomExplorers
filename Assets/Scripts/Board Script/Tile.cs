using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Direction { North, East, South, West, None }

public enum TileType { Normal, GainPoint, DropPoint, Home , Quiz, Portal, Reroll}

[DefaultExecutionOrder(-30)]
public class Tile : MonoBehaviour
{
    // Boolean flags to indicate available directions
    [SerializeField] private Tile northTile;
    [SerializeField] private Tile eastTile;
    [SerializeField] private Tile southTile;
    [SerializeField] private Tile westTile;


    // Boolean flag for tile propertiers
    [SerializeField] private int HomeplayerID; //for home

    // special tile attributes, normal by default
    [SerializeField] private TileType tileType = TileType.Normal;

    // Player ID of the players currently on the tile
    [SerializeField] private List<int> _TilePlayerIDs = new List<int>();
    public List<int> TilePlayerIDs
    {
        get => _TilePlayerIDs;
        set => _TilePlayerIDs = value;
    }


    void Start()
    {
        if(HomeplayerID != 0 && HomeplayerID <= PlayerManager.Instance.numOfPlayers)
        {
            AddPlayer(HomeplayerID);
        }
    }

    public int GetHomePlayerID()
    {
        return HomeplayerID;
    }

    // Return all valid directions (used for crossroads)
    public List<Direction> GetAllAvailableDirections()
    {
        List<Direction> validDirections = new List<Direction>();
        if (northTile != null) validDirections.Add(Direction.North);
        if (eastTile != null) validDirections.Add(Direction.East);
        if (southTile != null) validDirections.Add(Direction.South);
        if (westTile != null) validDirections.Add(Direction.West);
        return validDirections;
    }

    public Tile GetConnectedTile(Direction direction)
    {
        return direction switch
        {
            Direction.North => northTile,
            Direction.East => eastTile,
            Direction.South => southTile,
            Direction.West => westTile,
            _ => null
        };
    }

    public TileType GetTileType()
    {
        return tileType;
    }

    public void AddPlayer(int playerID)
    {
        if (!TilePlayerIDs.Contains(playerID))
        {
            TilePlayerIDs.Add(playerID);
        }
    }

    public void RemovePlayer(int playerID)
    {
        TilePlayerIDs.Remove(playerID);
    }


}

