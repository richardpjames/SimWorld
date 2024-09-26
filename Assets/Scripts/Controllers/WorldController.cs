using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldController : MonoBehaviour
{
    [Header("Tile Configuration")]
    [SerializeField] private TerrainTileConfiguration terrainTileConfiguration;
    [SerializeField] private StructureTileConfiguration structureTileConfiguration;
    [SerializeField] private FloorTileConfiguration floorTileConfiguration;
    [Header("Tilemaps")]
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap structureTilemap;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    public World World { get; private set; }

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
        terrainTilemap.GetComponent<Tilemap>().ClearAllTiles();
        structureTilemap.GetComponent<Tilemap>().ClearAllTiles();
        floorTilemap.GetComponent<Tilemap>().ClearAllTiles();

        // Initialise a new world
        World = new World(name, size);
        // Generate random biomes
        World.GenerateBiomes(waves, scale);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                TileBase tile = terrainTileConfiguration.GetTile(World.GetTerrain(new Vector2Int(x, y)).TerrainType);
                terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(World.Size.x / 2, World.Size.y / 2, Camera.main.transform.position.z);
    }

    /// <summary>
    /// Gets the floor at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Floor GetFloor(Vector2Int position)
    {
        return World.GetFloor(position);
    }

    /// <summary>
    /// Installs a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="floor"></param>
    public void InstallFloor(Vector2Int position, Floor floor)
    {
        bool success = World.InstallFloor(position, floor);
        // If the floor was installed, then update the tilemap
        if (success)
        {
            // Update the floor tile based on new information
            TileBase tile = floorTileConfiguration.GetTile(floor.FloorType);
            // Apply the tile to the tilemap
            floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    /// <summary>
    /// Removes a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    public void RemoveFloor(Vector2Int position)
    {
        bool success = World.RemoveFloor(position);
        // If the floor was removed then update the tilemap
        if (success)
        {
            floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
    }

    /// <summary>
    /// Gets the structure at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Structure GetStructure(Vector2Int position)
    {
        return World.GetStructure(position);
    }

    /// <summary>
    /// Installs a structure at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="structure"></param>
    public void InstallStructure(Vector2Int position, Structure structure)
    {
        bool success = World.InstallStructure(position, structure);
        // If the floor was installed, then update the tilemap
        if (success)
        {
            TileBase tile = structureTileConfiguration.GetTile(structure.StructureType);
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    /// <summary>
    /// Removes a floor at the specified position and also updates the graphics.
    /// </summary>
    /// <param name="position"></param>
    public void RemoveStructure(Vector2Int position)
    {
        bool success = World.RemoveStructure(position);
        // If the floor was removed then update the tilemap
        if (success)
        {
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);
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
        if (World == null || worldGrid == null)
        {
            return Vector2Int.zero;
        }
        // Get the correct position from the grid
        Vector3 position = worldGrid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, World.Size.x - 1);
        position.y = Mathf.Clamp(position.y, 0, World.Size.y - 1);
        return new Vector2Int((int)position.x, (int)position.y);
    }

    /// <summary>
    /// Get the terrain at a particular position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Terrain GetTerrain(Vector2Int position)
    {
        return World.GetTerrain(position);
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
        bool success = World.GetTerrain(position).SetType(type);
        if (success)
        {
            // Update the terrain tile based on new information
            TileBase tile = terrainTileConfiguration.GetTile(type);
            // Apply the tile to the tilemap
            terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }
}
