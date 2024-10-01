using System;
using System.Collections.Generic;
using UnityEngine;


public class World
{
    // This holds our world
    private Dictionary<Vector3Int, WorldTile> _worldTiles;
    public Vector2Int Size { get; private set; }
    public string Name { get; private set; }
    // Lets others know that a tile at the specified position is updated
    public Action<Vector2Int> OnTileUpdated;

    private PrefabManager _prefab { get => PrefabManager.Instance; }

    public World(string name, Vector2Int size)
    {
        // Store the name, width and height of the world
        this.Name = name;
        this.Size = size;
        // Initialise the array of tiles
        _worldTiles = new Dictionary<Vector3Int, WorldTile>();
        // Creates a world map with the height and width specified
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                Vector3Int lookup = new Vector3Int(x, y, (int)WorldLayer.Terrain);
                // Default each tile to be grass in the first instance
                _worldTiles.Add(lookup, _prefab.Grass);
            }
        }
    }

    public void GenerateBiomes(Wave[] waves, float scale)
    {
        // Generate a random offset
        Vector2 offset = new Vector2(UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500));
        // Generate a heightmap for the biomes
        float[,] heightMap = NoiseGenerator.Generate(Size, scale, waves, offset);
        // Update the tiles affected looping over the width
        for (int x = 0; x < Size.x; x++)
        {
            // Then loop over the height
            for (int y = 0; y < Size.y; y++)
            {
                Vector3Int lookup = new Vector3Int(x, y, (int)WorldLayer.Terrain);
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    _worldTiles[lookup] = _prefab.Water;
                }
                // Then sand as the height increases
                else if (heightMap[x, y] < 0.3)
                {
                    _worldTiles[lookup] = _prefab.Sand;
                }
                // All remaining tiles are grass
                else
                {
                    _worldTiles[lookup] = _prefab.Grass;
                }
            }
        }
    }

    public WorldTile GetWorldTile(Vector2Int position, WorldLayer layer)
    {
        // Get the lookup by casting the layer as the z position
        Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
        // If the position is within the world and the lookup exists in the dictionary
        if (CheckBounds(position) && _worldTiles.ContainsKey(lookup))
        {
            // Return the world tile
            return _worldTiles[lookup];
        }
        // Otherwise null
        return null;

    }

    public void UpdateWorldTile(Vector2Int position, WorldTile worldTile)
    {
        // Get the lookup by casting the layer as the z position
        Vector3Int lookup = new Vector3Int(position.x, position.y, (int)worldTile.Layer);
        // Check if we are out of bounds or trying to build on water
        if (CheckBounds(position) && worldTile.CheckValidity(this, position))
        {
            _worldTiles.Add(lookup, worldTile);
            OnTileUpdated?.Invoke(position);
        }
    }

    public void RemoveWorldTile(Vector2Int position, WorldLayer layer)
    {
        // Get the lookup by casting the layer as the z position
        Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
        if (_worldTiles.ContainsKey(lookup))
        {
            _worldTiles.Remove(lookup);
            OnTileUpdated?.Invoke(position);
        }
    }

    public bool CheckBounds(Vector2Int position)
    {
        return !(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y);
    }

}
