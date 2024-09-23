using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    // Represents the possible terrain types for a tile
    public enum TileType { Grass, Sand, Water }
    // Holds the type for this particular tile
    public TileType Type;

    // Constructor takes a base terrain for the tile
    public Tile(TileType type)
    {
        Type = type;
    }
}
