using System.Collections.Generic;
using UnityEngine;

public enum Direction { North, East, South, West, None }

public class Tile : MonoBehaviour
{
    public string tileName;

    // Boolean flags to indicate available directions
    public bool hasNorth;
    public bool hasEast;
    public bool hasSouth;
    public bool hasWest;

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
}

