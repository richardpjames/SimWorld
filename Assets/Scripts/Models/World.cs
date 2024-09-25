using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
    // This holds our world
    private Dictionary<Vector2Int, Square> squares;
    public Vector2Int Size { get; private set; }
    public string Name { get; private set; }

    public World(string name, Vector2Int size)
    {
        // Store the name, width and height of the world
        this.Name = name;
        this.Size = size;
        // Initialise the array of tiles
        squares = new Dictionary<Vector2Int, Square>();
        // Creates a world map with the height and width specified
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                // Default each tile to be grass in the first instance
                squares.Add(new Vector2Int(x, y), new Square(this, new Vector2Int(x, y), TerrainType.Grass));
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
        float[,] heightMap = NoiseGenerator.Generate(Size, scale, waves, offset);
        // Update the tiles affected looping over the width
        for (int x = 0; x < Size.x; x++)
        {
            // Then loop over the height
            for (int y = 0; y < Size.y; y++)
            {
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    GetSquare(new Vector2Int(x,y)).SetType(TerrainType.Water);
                }
                // Then sand as the height increases
                else if (heightMap[x, y] < 0.3)
                {
                    GetSquare(new Vector2Int(x, y)).SetType(TerrainType.Sand);
                }
                // All remaining tiles are grass
                else
                {
                    GetSquare(new Vector2Int(x, y)).SetType(TerrainType.Grass);
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
    public Square GetSquare(Vector2Int position)
    {
        // Check for whether we are out of bounds
        if(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y)
        {
            return null;
        }
        // If not, then return the square at this position
        return squares[position];
    }

}
