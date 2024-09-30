using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain : TileType
{
    // Holds the type for this particular tile
    public TerrainType TerrainType { get; set; }

    // Constructor takes a base terrain for the tile
    public Terrain(TerrainType type, TileBase tile, float movementCost = 1)
    {
        this.TerrainType = type;
        this.Tile = tile;
        this.MovementCost = movementCost;
    }

    /// <summary>
    /// Sets the terrain type for this particular tile
    /// </summary>
    /// <param name="type">The terrain type</param>
    public bool SetType(TerrainType type, TileBase tile)
    {
        TerrainType = type;
        Tile = tile;
        return true;
    }
}
