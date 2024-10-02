using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting.IonicZip;
using UnityEngine;
using Random = UnityEngine.Random;


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
                // Add grass to all tiles
                UpdateWorldTile(new Vector2Int(x, y), _prefab.Grass);
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Water);
                }
                // Then sand as the height increases
                if (heightMap[x, y] < 0.3)
                {
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Sand);
                }
                // Random chance (2%) that a rock is placed here (and avoid trees)
                int random = Random.Range(0, 100);
                if (random > 98)
                {
                    // Place a tree structure
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Rock);
                }
                // Add trees to high land - at a 60% chance (makes them clump together)
                if (heightMap[x, y] > 0.7)
                {
                    // Generate random number
                    random = Random.Range(0, 100);
                    // 40% chance that there are no trees (skip this)
                    if (random > 39)
                    {
                        // Place a tree structure
                        UpdateWorldTile(new Vector2Int(x, y), _prefab.Tree);
                    }
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
        // Create a new instance of the provided tile so as not to link together
        WorldTile newInstance = worldTile.NewInstance();
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + newInstance.MinX);
        int maxX = Mathf.Max(position.x, position.x + newInstance.MaxX);
        int minY = Mathf.Min(position.y, position.y + newInstance.MinY);
        int maxY = Mathf.Max(position.y, position.y + newInstance.MaxY);
        // Remove any reserved tiles
        for (int x = minX; x < maxX; x++)
        {
            // Remove any reserved tiles
            for (int y = minY; y < maxY; y++)
            {
                // First remove any reserving tiles 
                WorldTile existingTile = GetWorldTile(new Vector2Int(x, y), newInstance.Layer);
                if (existingTile != null && existingTile.GetType() == typeof(Reserved))
                {
                    RemoveWorldTile(new Vector2Int(x, y), existingTile.Layer);
                }
            }
        }
        // Now check validity and build if possible
        if (newInstance.CheckValidity(this, position))
        {
            // Loop through the width and height of the object from the start position
            // Remove any reserved tiles
            for (int x = minX; x < maxX; x++)
            {
                // Remove any reserved tiles
                for (int y = minY; y < maxY; y++)
                {
                    // Get the lookup by casting the layer as the z position
                    Vector3Int lookup = new Vector3Int(x, y, (int)newInstance.Layer);
                    if (_worldTiles.ContainsKey(lookup))
                    {
                        _worldTiles[lookup] = newInstance;
                    }
                    else
                    {
                        // Then add the tile to the dictionary in each position
                        _worldTiles.Add(lookup, newInstance);
                    }
                }
            }
            // Set the base position for only the requested position to help with graphics
            newInstance.BasePosition = position;
            OnTileUpdated?.Invoke(position);
        }
    }

    public void RemoveWorldTile(Vector2Int position, WorldLayer layer)
    {
        // Get the lookup by casting the layer as the z position
        Vector3Int lookup = new Vector3Int(position.x, position.y, (int)layer);
        if (_worldTiles.ContainsKey(lookup))
        {
            WorldTile worldTile = GetWorldTile(position, layer);
            // Find the min and max X and Y values to loop between
            int minX = Mathf.Min(worldTile.BasePosition.x, worldTile.BasePosition.x + worldTile.MinX);
            int maxX = Mathf.Max(worldTile.BasePosition.x, worldTile.BasePosition.x + worldTile.MaxX);
            int minY = Mathf.Min(worldTile.BasePosition.y, worldTile.BasePosition.y + worldTile.MinY);
            int maxY = Mathf.Max(worldTile.BasePosition.y, worldTile.BasePosition.y + worldTile.MaxY);
            // Remove any associated tiles by looping over the x and y
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    // Get the lookup by casting the layer as the z position
                    lookup = new Vector3Int(x, y, (int)worldTile.Layer);
                    if (_worldTiles.ContainsKey(lookup))
                    {
                        _worldTiles.Remove(lookup);
                        OnTileUpdated?.Invoke(new Vector2Int(x, y));
                    }
                }
            }
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
            if (_worldTiles.ContainsKey(lookup) && _worldTiles[lookup].BuildingAllowed == false)
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
