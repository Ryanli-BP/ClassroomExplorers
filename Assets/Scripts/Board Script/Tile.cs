using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Direction { North, East, South, West, None }

public enum TileType { Normal, GainPoint, DropPoint, Home , Quiz, Portal, Reroll}

public class Tile : MonoBehaviour
{
    // Boolean flags to indicate available directions
    [SerializeField] private bool hasNorth;
    [SerializeField] private bool hasEast;
    [SerializeField] private bool hasSouth;
    [SerializeField] private bool hasWest;


    // Boolean flag for tile propertiers
    [SerializeField] private int HomeplayerID; //for home

    // special tile attributes, normal by default
    [SerializeField] private TileType tileType = TileType.Normal;

    // Player ID of the players currently on the tile
    [SerializeField] public List<int> TilePlayerIDs { get; private set; } = new List<int>();

    void Start()
    {
        if(HomeplayerID != 0)
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
        if (hasNorth) validDirections.Add(Direction.North);
        if (hasEast) validDirections.Add(Direction.East);
        if (hasSouth) validDirections.Add(Direction.South);
        if (hasWest) validDirections.Add(Direction.West);

        return validDirections;
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

