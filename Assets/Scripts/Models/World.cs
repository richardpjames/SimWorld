using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
    // This holds our world
    private Dictionary<Vector2Int, Terrain> terrains;
    private Dictionary<Vector2Int, Structure> structures;
    private Dictionary<Vector2Int, Floor> floors;
    public Vector2Int Size { get; private set; }
    public string Name { get; private set; }

    public World(string name, Vector2Int size)
    {
        // Store the name, width and height of the world
        this.Name = name;
        this.Size = size;
        // Initialise the array of tiles
        terrains = new Dictionary<Vector2Int, Terrain>();
        structures = new Dictionary<Vector2Int, Structure>();
        floors = new Dictionary<Vector2Int, Floor>();
        // Creates a world map with the height and width specified
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                // Default each tile to be grass in the first instance
                terrains.Add(new Vector2Int(x, y), new Terrain(TerrainType.Grass));
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
                    GetTerrain(new Vector2Int(x, y)).SetType(TerrainType.Water);
                }
                // Then sand as the height increases
                else if (heightMap[x, y] < 0.3)
                {
                    GetTerrain(new Vector2Int(x, y)).SetType(TerrainType.Sand);
                }
                // All remaining tiles are grass
                else
                {
                    GetTerrain(new Vector2Int(x, y)).SetType(TerrainType.Grass);
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
    public Terrain GetTerrain(Vector2Int position)
    {
        // Check for whether we are out of bounds
        if (CheckBounds(position) && terrains.ContainsKey(position))
        {
            // If not then return the terrain
            return terrains[position];
        }
        // If we are, then return null
        return null;
    }
    // Get the structure at a position
    public Structure GetStructure(Vector2Int position)
    {
        if (CheckBounds(position) && structures.ContainsKey(position))
        {
            return structures[position];
        }
        return null;
    }

    /// <summary>
    /// Places a structure into the tile
    /// </summary>
    /// <param name="type">The structure to be installed /param>
    public bool InstallStructure(Vector2Int position, Structure structure)
    {
        if (CheckBounds(position) && GetTerrain(position).TerrainType != TerrainType.Water && !structures.ContainsKey(position))
        {
            structures.Add(position, structure);
            return true;
        }
        // If the floor could not be installed
        return false;
    }
    /// <summary>
    /// Removes any structures installed on this tile
    /// </summary>
    public bool RemoveStructure(Vector2Int position)
    {
        if (structures.ContainsKey(position))
        {
            structures.Remove(position);
            return true;
        }
        return false;
    }

    // Get the floor at a position
    public Floor GetFloor(Vector2Int position)
    {
        if (CheckBounds(position) && floors.ContainsKey(position))
        {
            return floors[position];
        }
        return null;
    }

    /// <summary>
    /// Places a floor into the tile
    /// </summary>
    /// <param name="floor">The floor to be installed </param>
    public bool InstallFloor(Vector2Int position, Floor floor)
    {
        if (CheckBounds(position) && GetTerrain(position).TerrainType != TerrainType.Water && !floors.ContainsKey(position))
        {
            floors.Add(position, floor);
            return true;
        }
        // If the floor could not be installed
        return false;
    }

    /// <summary>
    /// Removes any floor installed on this tile
    /// </summary>
    public bool RemoveFloor(Vector2Int position)
    {
        if (floors.ContainsKey(position))
        {
            floors.Remove(position);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns false if the vector falls outside of the world
    /// </summary>
    /// <returns></returns>
    private bool CheckBounds(Vector2Int position)
    {
        return !(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y);
    }

}
