using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class World : MonoBehaviour
{
    [Header("Biome Generation")]
    [SerializeField] private Wave[] _waves;
    [SerializeField] private float _scale;
    [SerializeField] private Vector2 _offset;
    [Header("Graphics Display")]
    [SerializeField] private Grid _grid;
    private PrefabFactory _prefab;
    private Inventory _inventory;

    // This holds our world
    private Dictionary<Vector3Int, WorldTile> _worldTiles;
    private Dictionary<WorldLayer, Tilemap> _tilemaps;
    private List<WorldTile> _needsUpdate;
    public List<WorldTile> Beds;
    public Vector2Int Size { get; private set; }
    public string Name { get; private set; }
    public NavigationGraph NavigationGraph { get; private set; }
    private bool _nagivationGraphInvalid = false;
    // Lets others know that a tile at the specified position is updated
    public Action<Vector2Int> OnTileUpdated;

    private GameManager _game { get => GameManager.Instance; }
    private void Start()
    {
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _prefab = GameObject.FindAnyObjectByType<PrefabFactory>();
        // Store the name, width and height of the world
        this.Name = _game.WorldName;
        this.Size = new Vector2Int(_game.WorldWidth, _game.WorldHeight);
        // Initialise the array of tiles
        _worldTiles = new Dictionary<Vector3Int, WorldTile>();
        // Initialise the list of tilemaps
        _tilemaps = new Dictionary<WorldLayer, Tilemap>();
        // Initialise the updates list
        _needsUpdate = new List<WorldTile>();
        // Initialise the beds list
        Beds = new List<WorldTile>();
        // Generate all required tilemaps
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            // Create a game object with a tilemap and a tilemap renderer
            GameObject tilemap_go = new GameObject($"Tilemap for {layer.ToString()}");
            Tilemap tilemap = tilemap_go.AddComponent<Tilemap>();
            TilemapRenderer tilemapRenderer = tilemap_go.AddComponent<TilemapRenderer>();
            // Add the game object to the grid
            tilemap_go.transform.SetParent(_grid.transform);
            // Set the sorting layer for the tilemap renderer
            tilemapRenderer.sortingLayerName = layer.ToString();
            // Set the sort order to allow for Y sorting
            tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            // Hold a reference to the tilemap in the dictionary
            _tilemaps.Add(layer, tilemap);
        }
        // Generate random biomes
        GenerateBiomes(_waves, _scale);
        // Build the starting structure
        int startingSturctureWidth = 6;
        int startingSturctureHeight = 6;
        // Add two to the area checked to make sure villagers can walk around the structure
        Vector2Int startingStructurePosition = FindClearArea(startingSturctureWidth + 2, startingSturctureHeight + 2);
        // Add one to the starting position in order to center in the wider check area
        BuildStartingStructure(new Vector2Int(startingStructurePosition.x + 1, startingStructurePosition.y + 1), startingSturctureWidth, startingSturctureHeight);
        // Add a goblin spawn point randomly
        Vector2Int goblinSpawnPosition = FindClearArea(2, 2);
        UpdateWorldTile(new Vector2Int(goblinSpawnPosition.x, goblinSpawnPosition.y + 1), _prefab.Create("Goblin Spawn Point"));
        // Generate the navigation graph
        NavigationGraph = new NavigationGraph();
        NavigationGraph.Calculate(this);
        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(startingStructurePosition.x, startingStructurePosition.y, Camera.main.transform.position.z);
    }
    private void Update()
    {
        if (_nagivationGraphInvalid)
        {
            NavigationGraph.Calculate(this);
        }
        // Loop through all of the items which require update
        for (int i = 0; i < _needsUpdate.Count; i++)
        {
            _needsUpdate[i].Update(Time.deltaTime * GameManager.Instance.TimeMultiplier);
        }
    }

    public Vector2Int GetNearestStructure(Vector2Int position, TileType type)
    {
        float minDistance = float.MaxValue;
        Vector2Int result = Vector2Int.zero;

        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                WorldTile tile = GetWorldTile(new Vector2Int(x, y), WorldLayer.Structure);
                if (tile != null && tile.Type == type && tile.BasePosition == new Vector2Int(x, y) && tile.Reserved == false)
                {
                    float distance = Vector2Int.Distance(position, new Vector2Int(x, y));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = new Vector2Int(x, y);
                    }
                }
            }
        }
        return result;
    }

    private void GenerateBiomes(Wave[] waves, float scale)
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
                UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Grass"));
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Water"));
                }
                // Then sand as the height increases
                if (heightMap[x, y] < 0.3)
                {
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Sand"));
                }
                // Random chance (2%) that a rock is placed here (and avoid trees)
                int random = Random.Range(0, 100);
                if (random > 98)
                {
                    // Place a tree structure
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Rock"));
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
                        UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Tree"));
                    }
                }
            }
        }
    }

    private Vector2Int FindClearArea(int width, int height)
    {
        bool found = false;
        Vector2Int position = Vector2Int.zero;
        while (found == false)
        {
            // Pick a random position
            position.x = Random.Range(0, Size.x);
            position.y = Random.Range(0, Size.y);
            // Initialise that we have found a spot
            found = true;
            // Check whether it can be built on based on width and height
            for (int x = position.x; x < position.x + width; x++)
            {
                for (int y = position.y; y < position.y + height; y++)
                {
                    if (!IsBuildable(new Vector2Int(x, y)) || !CheckBounds(new Vector2Int(x, y)))
                    {
                        found = false;
                    }
                }
            }
        }
        return position;
    }

    private void BuildStartingStructure(Vector2Int position, int width, int height)
    {
        // Place floors and walls in a loop
        for (int x = position.x; x < position.x + width; x++)
        {
            for (int y = position.y; y < position.y + height; y++)
            {
                // For all tiles we want to create the floor
                UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Wooden Floor"));
                // Around the edges we create a wall
                if (x == position.x || y == position.y || x == position.x + width - 1 || y == position.y + height - 1)
                {
                    // Leave a gap for the door
                    if (x == (int)(position.x + (width / 2)) && y == (int)position.y) continue;
                    // Otherwise place a wall
                    UpdateWorldTile(new Vector2Int(x, y), _prefab.Create("Wooden Wall"));
                }
            }
        }
        // Add the door
        UpdateWorldTile(new Vector2Int((int)position.x + (width / 2), position.y), _prefab.Create("Wooden Door"));
        // Create two beds in the corner
        UpdateWorldTile(new Vector2Int(position.x + 1, position.y + height - 2), _prefab.Create("Wooden Bed"));
        UpdateWorldTile(new Vector2Int(position.x + width - 2, position.y + height - 2), _prefab.Create("Wooden Bed"));

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
                if (existingTile != null && existingTile.Type == TileType.Reserved)
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
                        // If the previous tile needed updates, then remove it
                        if (_worldTiles[lookup].RequiresUpdate)
                        {
                            _needsUpdate.Remove(_worldTiles[lookup]);
                        }
                        // Remove from our list of beds if needed
                        if (_worldTiles[lookup].Type == TileType.Bed)
                        {
                            Beds.Remove(_worldTiles[lookup]);
                        }
                        // Set the new tile
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
            // If the new tile needs updates then add it to the list
            if (newInstance.RequiresUpdate)
            {
                _needsUpdate.Add(newInstance);
            }
            // If this is a bed then add it to our list
            if (newInstance.Type == TileType.Bed)
            {
                Beds.Add(newInstance);
            }
            OnTileUpdated?.Invoke(position);
            // Update the correct tilemap
            Matrix4x4 matrix = Matrix4x4.Rotate(newInstance.Rotation);
            _tilemaps[newInstance.Layer].SetTile(new Vector3Int(position.x, position.y, 0), newInstance.Tile);
            _tilemaps[newInstance.Layer].SetTransformMatrix(new Vector3Int(position.x, position.y, 0), matrix);
            // Force a recalculation of the navigation graph
            _nagivationGraphInvalid = true;
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
                        // If this is the base of the object, then collect any yield from it
                        if (_worldTiles[lookup].BasePosition == new Vector2Int(x, y))
                        {
                            _inventory.Add(_worldTiles[lookup].Yield);
                            // If it requires update then remove from the list
                            if (_worldTiles[lookup].RequiresUpdate)
                            {
                                _needsUpdate.Remove(_worldTiles[lookup]);
                            }
                            // Remove from our list of beds if needed
                            if (_worldTiles[lookup].Type == TileType.Bed)
                            {
                                Beds.Remove(_worldTiles[lookup]);
                            }
                        }
                        _worldTiles.Remove(lookup);
                        OnTileUpdated?.Invoke(new Vector2Int(x, y));
                        // Remove from the tilemap
                        _tilemaps[worldTile.Layer].SetTile(new Vector3Int(x, y, 0), null);
                        _tilemaps[WorldLayer.Demolition].SetTile(new Vector3Int(x, y, 0), null);
                        // Force a recalculation of the navigation graph
                        _nagivationGraphInvalid = true;
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

    public bool IsInside(Vector2Int position)
    {
        // Start a queue with the current positition
        Queue<Vector2Int> nodes = new Queue<Vector2Int>();
        nodes.Enqueue(position);
        // Keep a list of nodes which have been checked to avoid duplication
        HashSet<Vector2Int> checkedNodes = new HashSet<Vector2Int>();
        // Keep looping while we have nodes to check
        while (nodes.Count > 0)
        {
            Vector2Int currentNode = nodes.Dequeue();
            // Don't continue if we have already checked this node
            if (checkedNodes.Contains(currentNode)) continue;
            checkedNodes.Add(currentNode);
            // Check conditions for if this node is inside
            bool inBounds = CheckBounds(currentNode);
            WorldTile floor = GetWorldTile(currentNode, WorldLayer.Floor);
            WorldTile structure = GetWorldTile(currentNode, WorldLayer.Structure);

            // If we hit a wall or a door, then we continue to the next loop iteration and don't add more tiles to check
            if (structure != null && (structure.Type == TileType.Wall || structure.Type == TileType.Door)) continue;
            // If we are out of bounds then return false
            if (!inBounds) return false;
            // If there is no floor then we are outside so return false
            if (floor == null || floor.Type == TileType.Reserved) return false;
            // Otherwise we are inside and must check the neighbours
            if (!checkedNodes.Contains(currentNode + Vector2Int.up)) nodes.Enqueue(currentNode + Vector2Int.up);
            if (!checkedNodes.Contains(currentNode + Vector2Int.right)) nodes.Enqueue(currentNode + Vector2Int.right);
            if (!checkedNodes.Contains(currentNode + Vector2Int.down)) nodes.Enqueue(currentNode + Vector2Int.down);
            if (!checkedNodes.Contains(currentNode + Vector2Int.left)) nodes.Enqueue(currentNode + Vector2Int.left);
        }

        return true;
    }
    public bool CheckBounds(Vector2Int position)
    {
        return !(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y);
    }

    public Sprite GetSprite(Vector2Int position, WorldLayer layer)
    {
        return _tilemaps[layer].GetSprite(new Vector3Int(position.x, position.y, 0));
    }

    public Vector2Int GetTilePosition(float x, float y)
    {
        // Get the correct position from the grid
        Vector3 position = _grid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, Size.x - 1);
        position.y = Mathf.Clamp(position.y, 0, Size.y - 1);
        return new Vector2Int((int)position.x, (int)position.y);
    }

}
