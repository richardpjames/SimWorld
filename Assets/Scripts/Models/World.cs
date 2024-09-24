using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
    // This holds our world
    private Tile[,] tiles;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string Name { get; private set; }

    public World(string name, int width, int height)
    {
        // Store the name, width and height of the world
        this.Name = name;
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
                tiles[x, y] = new Tile(x, y, TileType.Grass);
            }
        }
    }

    /// <summary>
    /// Generates random biomes within the map
    /// </summary>
    public void GenerateBiomes(Wave[] waves, float scale)
    {
        // Generate a random offset
        Vector2 offset = new Vector2(Random.Range(0, 500), Random.Range(0, 500));
        // Generate a heightmap for the biomes
        float[,] heightMap = NoiseGenerator.Generate(Width, Height, scale, waves, offset);
        // Update the tiles affected looping over the width
        for (int x = 0; x < Width; x++)
        {
            // Then loop over the height
            for (int y = 0; y < Height; y++)
            {
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    GetTile(x, y).SetType(TileType.Water);
                }
                // Then sand as the height increases
                else if (heightMap[x, y] < 0.3)
                {
                    GetTile(x, y).SetType(TileType.Sand);
                }
                // All remaining tiles are grass
                else
                {
                    GetTile(x, y).SetType(TileType.Grass);
                }
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
