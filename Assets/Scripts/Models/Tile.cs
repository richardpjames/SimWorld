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

    // Reference to the position in the world
    public int X { get; private set; }
    public int Y { get ; private set; }

    // Keep track of any objects on the tile
    public Structure InstalledStructure { get; private set; }

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

    /// <summary>
    /// Places a structure into the tile
    /// </summary>
    /// <param name="type">The structure to be installed /param>
    public void InstallStructure(Structure structure)
    {
        // Don't allow for building on water
        if (Type == TileType.Water)
        {
            return;
        }
        InstalledStructure = structure;
        // Trigger an event to say that the tile is updated
        OnTileUpdated?.Invoke(X, Y);
    }
    /// <summary>
    /// Removes any structures installed on this tile
    /// </summary>
    public void RemoveStructure()
    {
        InstalledStructure = null;
        // Trigger an event to say that the tile is updated
        OnTileUpdated?.Invoke(X, Y);
    }

}
