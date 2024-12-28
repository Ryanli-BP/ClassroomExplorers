using System.Collections.Generic;
using UnityEngine;

public enum Direction { North, East, South, West, None }

public enum TileType { Normal, GainPoint, DropPoint, Home }

public class Tile : MonoBehaviour
{
    // Boolean flags to indicate available directions
    [SerializeField] private bool hasNorth;
    [SerializeField] private bool hasEast;
    [SerializeField] private bool hasSouth;
    [SerializeField] private bool hasWest;


    // Boolean flag for tile propertiers
    [SerializeField] private bool isHome;
    [SerializeField] private int playerID; //for home

    // special tile attributes, normal by default
    [SerializeField] private TileType tileType = TileType.Normal;

    

    public int GetPlayerID()
    {
        return playerID;
    }

    // Return all valid directions (used for crossroads)
    public List<Direction> GetAllAvailableDirections(Direction fromDirection)
    {
        List<Direction> validDirections = new List<Direction>();
        if (hasNorth && fromDirection != Direction.South) validDirections.Add(Direction.North);
        if (hasEast && fromDirection != Direction.West) validDirections.Add(Direction.East);
        if (hasSouth && fromDirection != Direction.North) validDirections.Add(Direction.South);
        if (hasWest && fromDirection != Direction.East) validDirections.Add(Direction.West);

        return validDirections;
    }

    public bool IsHome()
    {
        return isHome;
    }

    public TileType GetTileType()
    {
        return tileType;
    }


}

