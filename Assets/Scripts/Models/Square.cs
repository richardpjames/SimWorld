using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Square
{
    // Reference to the world in which the square is contained
    private World world;
    // To let others know when a tile is updated
    public Action<Vector2Int> OnSquareUpdated;
    public Action<Vector2Int> OnStructureUpdated;
    public Action<Vector2Int> OnFloorUpdated;

    // Holds the type for this particular tile
    public TerrainType TerrainType { get; private set; }
    // Reference to the position in the world
    public Vector2Int Position { get; private set; }
    // Keep track of any installed objects on the tile
    public Structure InstalledStructure { get; private set; }
    // Keep track of any intalled floors on the tile
    public Floor InstalledFloor { get; private set; }

    // Reference to adjacent squares
    public Square SquareNorth { get => world.GetSquare(new Vector2Int(Position.x, Position.y + 1)); }
    public Square SquareEast { get => world.GetSquare(new Vector2Int(Position.x + 1, Position.y)); }
    public Square SquareSouth { get => world.GetSquare(new Vector2Int(Position.x, Position.y - 1)); }
    public Square SquareWest { get => world.GetSquare(new Vector2Int(Position.x - 1, Position.y)); }

    // Constructor takes a base terrain for the tile
    public Square(World world, Vector2Int position, TerrainType type)
    {
        this.world = world;
        this.Position = position;
        this.TerrainType = type;
    }

    /// <summary>
    /// Sets the terrain type for this particular tile
    /// </summary>
    /// <param name="type">The terrain type</param>
    public void SetType(TerrainType type)
    {
        TerrainType = type;
        if (type == TerrainType.Water)
        {
            RemoveStructure();
            RemoveFloor();
        }
        // Trigger an event to say that the tile is updated
        OnSquareUpdated?.Invoke(Position);
    }

    /// <summary>
    /// Places a structure into the tile
    /// </summary>
    /// <param name="type">The structure to be installed /param>
    public void InstallStructure(Structure structure)
    {
        // Don't allow for building on water
        if (TerrainType == TerrainType.Water)
        {
            return;
        }
        InstalledStructure = structure;
        structure.BaseSquare = this;
        // Trigger an event to say that the tile is updated
        OnStructureUpdated?.Invoke(Position);
    }
    /// <summary>
    /// Removes any structures installed on this tile
    /// </summary>
    public void RemoveStructure()
    {
        InstalledStructure = null;
        // Trigger an event to say that the tile is updated
        OnStructureUpdated?.Invoke(Position);
    }

    /// <summary>
    /// Places a floor into the tile
    /// </summary>
    /// <param name="floor">The floor to be installed /param>
    public void InstallFloor(Floor floor)
    {
        // Don't allow for building on water
        if (TerrainType == TerrainType.Water)
        {
            return;
        }
        InstalledFloor = floor;
        floor.BaseSquare = this;
        // Trigger an event to say that the tile is updated
        OnFloorUpdated?.Invoke(Position);
    }
    /// <summary>
    /// Removes any floor installed on this tile
    /// </summary>
    public void RemoveFloor()
    {
        InstalledFloor = null;
        // Trigger an event to say that the tile is updated
        OnFloorUpdated?.Invoke(Position);
    }

}
