using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
    [Header("World Generation")]
    [SerializeField] private string worldName = "Default";
    [SerializeField] private int height;
    [SerializeField] private int width;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    public World World { get; private set; }

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }
    public string WorldName { get => worldName; private set => worldName = value; }

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

    // Start is called before the first frame update
    void Start()
    {
        // Initialise the world
        Initialize(WorldName, new Vector2Int(width, height));
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

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                TileBase tile = terrainTileConfiguration.GetTile(World.GetSquare(new Vector2Int(x, y)).TerrainType);
                terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tile);
                // Subscribe to any square or structure changes
                World.GetSquare(new Vector2Int(x, y)).OnSquareUpdated += SquareUpdated;
                World.GetSquare(new Vector2Int(x, y)).OnStructureUpdated += StructureUpdated;
                World.GetSquare(new Vector2Int(x, y)).OnFloorUpdated += FloorUpdated;
            }
        }
        // Generate random biomes
        World.GenerateBiomes(waves, scale);
        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(World.Size.x / 2, World.Size.y / 2, Camera.main.transform.position.z);
    }

    /// <summary>
    /// This should be called whenever a tile is updated at the specified position
    /// </summary>
    /// <param name="position">The world position as a Vector2</param>
    private void SquareUpdated(Vector2Int position)
    {
        // Update the terrain tile based on new information
        TileBase tile = terrainTileConfiguration.GetTile(World.GetSquare(position).TerrainType);
        // Apply the tile to the tilemap
        terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
    }

    /// <summary>
    /// To be called whenever a structure is updated at the specified position
    /// </summary>
    /// <param name="position">The world position as a Vector2</param>
    private void StructureUpdated(Vector2Int position)
    {
        Debug.Log("Structure updated");
        // If a structure is removed then set the structure tile to null
        if (World.GetSquare(position).InstalledStructure == null)
        {
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        // If a structure is added then find the correct tile from the configuration
        else
        {
            Debug.Log("Adding Structure to Map");
            TileBase tile = structureTileConfiguration.GetTile(World.GetSquare(position).InstalledStructure.StructureType);
            Debug.Log($"Adding tile: {tile.name}");
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }

    private void FloorUpdated(Vector2Int position)
    {
        if (World.GetSquare(position).InstalledFloor == null)
        {
            floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        else
        {
            // Update the floor tile based on new information
            TileBase tile = floorTileConfiguration.GetTile(World.GetSquare(position).InstalledFloor.FloorType);
            // Apply the tile to the tilemap
            floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
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
        // Get the correct position from the grid
        Vector3 position = worldGrid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, World.Size.x - 1);
        position.y = Mathf.Clamp(position.y, 0, World.Size.y - 1);
        return new Vector2Int((int)position.x, (int)position.y);
    }

    /// <summary>
    /// Set the type of a tile at a sepecific world location
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="type">The type (e.g. sand)</param>
    public void SetSquareType(Vector2Int position, TerrainType type)
    {
        // Set the tile to the new type
        World.GetSquare(position).SetType(type);
    }
}
