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
                Vector3Int lookup = new Vector3Int(x, y, (int)WorldLayer.Grass);
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
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    Vector3Int lookup = new Vector3Int(x, y, (int)WorldLayer.Water);
                    _worldTiles[lookup] = _prefab.Water;
                }
                // Then sand as the height increases
                if (heightMap[x, y] < 0.3)
                {
                    Vector3Int lookup = new Vector3Int(x, y, (int)WorldLayer.Sand);
                    _worldTiles[lookup] = _prefab.Sand;
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

    public float MovementCost(Vector2Int position)
    {
        float cost = 1f;
        // Check all layers for tiles
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
            // Check if there is a tile in the dictionary and take into account its movement cost
            if (_worldTiles.ContainsKey(lookup))
            {
                cost *= _worldTiles[lookup].MovementCost;
            }
        }
        return cost;
    }
    public bool IsBuildable(Vector2Int position)
    {
        // Check all layers for an item that is not buildable
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
            // Check if there is a tile in the dictionary and it bans building
            if(_worldTiles.ContainsKey(lookup) && _worldTiles[lookup].BuildingAllowed == false)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsWalkable(Vector2Int position)
    {
        // Check all layers for an item that is not walkable
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
            // Check if there is a tile in the dictionary and it bans walking
            if (_worldTiles.ContainsKey(lookup) && _worldTiles[lookup].Walkable == false)
            {
                return false;
            }
        }
        return true;
    }
    public bool CheckBounds(Vector2Int position)
    {
        return !(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y);
    }

}
