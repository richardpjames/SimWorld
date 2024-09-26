using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Terrain
{
    // Holds the type for this particular tile
    public TerrainType TerrainType { get; set; }
    // Reference to the position in the world

    // Constructor takes a base terrain for the tile
    public Terrain(TerrainType type)
    {
        this.TerrainType = type;
    }

    /// <summary>
    /// Sets the terrain type for this particular tile
    /// </summary>
    /// <param name="type">The terrain type</param>
    public bool SetType(TerrainType type)
    {
        TerrainType = type;
        return true;
    }
}
