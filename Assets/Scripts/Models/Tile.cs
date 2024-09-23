using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Action<int,int> OnTileUpdated;
    // Represents the possible terrain types for a tile

    // Holds the type for this particular tile
    public TileType Type { get; private set; }

    public int X { get; private set; }
    public int Y { get ; private set; }

    // Constructor takes a base terrain for the tile
    public Tile(int x, int y, TileType type)
    {
        this.X = x;
        this.Y = y;
        this.Type = type;
    }

    /// <summary>
    /// Sets the terrain type for this particular tile
    /// </summary>
    /// <param name="type">The terrain type</param>
    public void SetType(TileType type)
    {
        Type = type;
        // Trigger an event to say that the tile is updated
        OnTileUpdated?.Invoke(X,Y);
    }
}
