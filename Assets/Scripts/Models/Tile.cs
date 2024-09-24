using System;
using UnityEngine;

public class Tile
{
    // To let others know when a tile is updated
    public Action<Vector2Int> OnTileUpdated;

    // Holds the type for this particular tile
    public TileType Type { get; private set; }

    // Reference to the position in the world
    public Vector2Int Position { get; private set; }

    // Keep track of any installed objects on the tile
    public Structure InstalledStructure { get; private set; }

    // Constructor takes a base terrain for the tile
    public Tile(Vector2Int position, TileType type)
    {
        this.Position = position;
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
        OnTileUpdated?.Invoke(Position);
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
        OnTileUpdated?.Invoke(Position);
    }
    /// <summary>
    /// Removes any structures installed on this tile
    /// </summary>
    public void RemoveStructure()
    {
        InstalledStructure = null;
        // Trigger an event to say that the tile is updated
        OnTileUpdated?.Invoke(Position);
    }

}
