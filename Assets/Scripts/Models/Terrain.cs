using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain : TileType
{
    // Holds the type for this particular tile
    public TerrainType TerrainType { get; set; }
    // Reference to the position in the world

    // Constructor takes a base terrain for the tile
    public Terrain(TerrainType type, TileBase tile)
    {
        this.TerrainType = type;
        this.Tile = tile;
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
