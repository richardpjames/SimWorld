using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldController : MonoBehaviour
{
    [Header("Tile Configuration")]
    [SerializeField] private TerrainDataConfiguration terrainTileConfiguration;
    [SerializeField] private StructureDataConfiguration structureDataConfiguration;
    [SerializeField] private FloorDataConfiguration floorDataConfiguration;
    [Header("Tilemaps")]
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap structureTilemap;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    // All access to the world is through the controller (no direct references)
    private World world;

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Creates a new game world (or resets the existing)
    /// </summary>
    public void Initialize(string name, Vector2Int size)
    {
        // Clear any tiles if there is a tilemap present
        GetTilemap<Terrain>().ClearAllTiles();
        GetTilemap<Structure>().ClearAllTiles();
        GetTilemap<Floor>().ClearAllTiles();

        // Initialise a new world
        world = new World(name, size);
        // Generate random biomes
        world.GenerateBiomes(waves, scale);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < world.Size.x; x++)
        {
            for (int y = 0; y < world.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                Terrain terrain = world.Get<Terrain>(new Vector2Int(x, y));
                TileBase tile = terrainTileConfiguration.GetTile(terrain.TerrainType);
                GetTilemap<Terrain>().SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(world.Size.x / 2, world.Size.y / 2, Camera.main.transform.position.z);
    }

    // TODO: Get rid of references to this throughout the game
    public World GetWorld()
    {
        return world;
    }

    /// <summary>
    /// Gets the size of the world as a vector
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetWorldSize()
    {
        return world.Size;
    }

    /// <summary>
    /// Gets the floor at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Floor GetFloor(Vector2Int position)
    {
        return world.Get<Floor>(position);
    }

    /// <summary>
    /// Installs a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="floor"></param>
    public void InstallFloor(Vector2Int position, Floor floor)
    {
        bool success = world.Install<Floor>(position, floor);
        // If the floor was installed, then update the tilemap
        if (success)
        {
            // Update the floor tile based on new information
            TileBase tile = floorDataConfiguration.GetTile(floor.FloorType);
            // Apply the tile to the tilemap
            GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    /// <summary>
    /// Removes a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    public void RemoveFloor(Vector2Int position)
    {
        bool success = world.Remove<Floor>(position);
        // If the floor was removed then update the tilemap
        if (success)
        {
            GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
    }

    /// <summary>
    /// Gets the structure at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Structure GetStructure(Vector2Int position)
    {
        return world.Get<Structure>(position);
    }

    /// <summary>
    /// Installs a structure at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="structure"></param>
    public void InstallStructure(Vector2Int position, Structure structure)
    {
        bool success = world.Install<Structure>(position, structure);
        // If the floor was installed, then update the tilemap
        if (success)
        {
            TileBase tile = structureDataConfiguration.GetTile(structure.StructureType);
            GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    /// <summary>
    /// Removes a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    public void RemoveStructure(Vector2Int position)
    {
        bool success = world.Remove<Structure>(position);
        // If the floor was removed then update the tilemap
        if (success)
        {
            GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
    }

    /// <summary>
    /// Takes a world position and determines which tile occupies that position (with an x, y coordinate returned).
    /// </summary>
    /// <param name="x">The x world position</param>
    /// <param name="y">The y world position</param>
    /// <returns>The position of the tile which occupies the requested position</returns>
    public Vector2Int GetSquarePosition(float x, float y)
    {
        // Check that the world is created
        if (world == null || worldGrid == null)
        {
            return Vector2Int.zero;
        }
        // Get the correct position from the grid
        Vector3 position = worldGrid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, world.Size.x - 1);
        position.y = Mathf.Clamp(position.y, 0, world.Size.y - 1);
        return new Vector2Int((int)position.x, (int)position.y);
    }

    /// <summary>
    /// Get the terrain at a particular position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Terrain GetTerrain(Vector2Int position)
    {
        return world.Get<Terrain>(position);
    }

    /// <summary>
    /// Set the type of a tile at a sepecific world location
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="type">The type (e.g. sand)</param>
    public void SetTerrainType(Vector2Int position, TerrainType type)
    {
        // Set the tile to the new type
        Terrain terrain = world.Get<Terrain>(position);
        bool success = terrain.SetType(type);
        if (success)
        {
            // Update the terrain tile based on new information
            TileBase tile = terrainTileConfiguration.GetTile(type);
            // Apply the tile to the tilemap
            GetTilemap<Terrain>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    /// <summary>
    /// Get the tilemap component for the provided type
    /// </summary>
    /// <typeparam name="Structure"></typeparam>
    /// <returns></returns>
    private Tilemap GetTilemap<T>()
    {
        // Return the correct tilemap based on the type
        if (typeof(T) == typeof(Structure)) return structureTilemap.GetComponent<Tilemap>();
        if (typeof(T) == typeof(Floor)) return floorTilemap.GetComponent<Tilemap>();
        if (typeof(T) == typeof(Terrain)) return terrainTilemap.GetComponent<Tilemap>();
        // For all other types return null
        return null;
    }

}
