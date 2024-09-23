using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    // This holds our world
    private Tile[,] tiles;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public World(int width, int height)
    {
        // Store the width and height of the world
        this.Width = width;
        this.Height = height;
        // Initialise the array of tiles
        tiles = new Tile[Width, Height];
        // Creates a world map with the height and width specified
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Default each tile to be grass in the first instance
                tiles[x, y] = new Tile(Tile.TileType.Grass);
            }
        }
    }

    /// <summary>
    /// Returns the tile within the world at the specified x and y coordinates.
    /// </summary>
    /// <param name="x">The x world position.</param>
    /// <param name="y">The y world position.</param>
    /// <returns>The Tile at the specified world position.</returns>
    public Tile GetTile(int x, int y)
    {
        return tiles[x, y];
    }

}
