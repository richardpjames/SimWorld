using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;


public class WorldController : MonoBehaviour
{
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private Sprite grassSprite;
    [SerializeField] private Sprite sandSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;
    [SerializeField] public string WorldName { get; private set; } = "Default";
    private Dictionary<TileType, Tile> worldTiles;
    private Dictionary<string, Tile> structureTiles;
    public World World { get; private set; }

    public Grid WorldGrid { get; private set; }
    private GameObject terrainTilemap;
    private GameObject structureTilemap;

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

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the dictionary of tile sprites
        worldTiles = new Dictionary<TileType, Tile>();
        // Add each of the sprite variations
        worldTiles.Add(TileType.Sand, ScriptableObject.CreateInstance<Tile>());
        worldTiles[TileType.Sand].sprite = sandSprite;
        worldTiles.Add(TileType.Water, ScriptableObject.CreateInstance<Tile>());
        worldTiles[TileType.Water].sprite = waterSprite;
        worldTiles.Add(TileType.Grass, ScriptableObject.CreateInstance<Tile>());
        worldTiles[TileType.Grass].sprite = grassSprite;
        // Add sprite variations for structures
        structureTiles = new Dictionary<string, Tile>();
        structureTiles.Add("Wall", ScriptableObject.CreateInstance<Tile>());
        structureTiles["Wall"].sprite = wallSprite;
        // Initialise the world
        Initialize(WorldName, new Vector2Int(width, height));
    }

    /// <summary>
    /// Creates a new game world (or resets the existing)
    /// </summary>
    public void Initialize(string name, Vector2Int size)
    {
        // Initialise grid and tilemap
        WorldGrid = new GameObject("World Grid").AddComponent<Grid>();
        WorldGrid.cellSize = new Vector3(1f, 1f, 1f);
        // Add the tilemap to the grid
        terrainTilemap = new GameObject("Terrain");
        terrainTilemap.AddComponent<Tilemap>();
        TilemapRenderer terrainRenderer = terrainTilemap.AddComponent<TilemapRenderer>();
        terrainRenderer.sortingLayerName = "Terrain";
        terrainTilemap.transform.SetParent(WorldGrid.transform, false);
        // Add another tilemap for structures
        structureTilemap = new GameObject("Structures");
        structureTilemap.AddComponent<Tilemap>();
        TilemapRenderer structureRenderer = structureTilemap.AddComponent<TilemapRenderer>();
        structureRenderer.sortingLayerName = "Structures";
        structureTilemap.transform.SetParent(WorldGrid.transform, false);
        // Initialise a new world
        World = new World(name, size);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
            {
                // Set the square type to grass initially
                terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), worldTiles[TileType.Grass]);
                // Subscribe to any tile changes
                World.GetTile(new Vector2Int(x, y)).OnTileUpdated += SquareUpdated;
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
        // Update the terrain for the tile
        terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), worldTiles[World.GetTile(position).Type]);
        // If a structure is removed then set the structure tile to null
        if (World.GetTile(position).InstalledStructure == null)
        {
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);

        }
        // If a structure is added then find the correct tile from the dictionary
        else
        {
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), structureTiles[World.GetTile(position).InstalledStructure.StructureType]);
        }
    }

    /// <summary>
    /// Takes a world position and determines which tile occupies that position (with an x, y coordinate returned).
    /// </summary>
    /// <param name="x">The x world position</param>
    /// <param name="y">The y world position</param>
    /// <returns>The position of the tile which occupies the requested position</returns>
    public Vector3Int GetTilePosition(float x, float y)
    {
        // Get the correct position from the grid
        Vector3 position = WorldGrid.WorldToCell(new Vector3(x,y,0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, World.Size.x);
        position.y = Mathf.Clamp(position.y, 0, World.Size.y);
        return new Vector3Int((int)position.x, (int)position.y,(int)position.z);
    }

    /// <summary>
    /// Set the type of a tile at a sepecific world location
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="type">The type (e.g. sand)</param>
    public void SetTileType(Vector2Int position, TileType type)
    {
        // Set the tile to the new type
        World.GetTile(position).SetType(type);
    }
}
