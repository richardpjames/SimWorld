using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;


public class WorldController : MonoBehaviour
{
    [Header("Sprite Configuration")]
    [SerializeField] private TerrainSpriteConfiguration terrainSpriteConfiguration;
    [SerializeField] private StructureSpriteConfiguration structureSpriteConfiguration;
    [Header("World Generation")]
    [SerializeField] private string worldName = "Default";
    [SerializeField] private int height;
    [SerializeField] private int width;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    public Grid WorldGrid { get; private set; }
    public World World { get; private set; }

    private GameObject terrainTilemap;
    private GameObject structureTilemap;

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
        if (terrainTilemap != null)
        {
            terrainTilemap.GetComponent<Tilemap>().ClearAllTiles();
        }
        if (structureTilemap != null)
        {
            structureTilemap.GetComponent<Tilemap>().ClearAllTiles();
        }
        // Initialise grid and tilemap
        WorldGrid = new GameObject("World Grid").AddComponent<Grid>();
        WorldGrid.cellSize = new Vector3(1f, 1f, 1f);
        // Add the tilemap to the grid
        terrainTilemap = new GameObject("Terrain");
        terrainTilemap.AddComponent<Tilemap>();
        // Add the renderer so that the tilemap appears and is on the right sorting layer
        TilemapRenderer terrainRenderer = terrainTilemap.AddComponent<TilemapRenderer>();
        terrainRenderer.sortingLayerName = "Terrain";
        // Add to the world grid
        terrainTilemap.transform.SetParent(WorldGrid.transform, false);
        // Add another tilemap for structures
        structureTilemap = new GameObject("Structures");
        structureTilemap.AddComponent<Tilemap>();
        // Add the renderer so that the tilemap appears and is on the right sorting layer
        TilemapRenderer structureRenderer = structureTilemap.AddComponent<TilemapRenderer>();
        structureRenderer.sortingLayerName = "Structures";
        // Add to the world grid
        structureTilemap.transform.SetParent(WorldGrid.transform, false);
        // Initialise a new world
        World = new World(name, size);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = terrainSpriteConfiguration.GetSprite(World.GetSquare(new Vector2Int(x, y)));
                terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tile);
                // Subscribe to any square or structure changes
                World.GetSquare(new Vector2Int(x, y)).OnSquareUpdated += SquareUpdated;
                World.GetSquare(new Vector2Int(x, y)).OnStructureUpdated += StructureUpdated;
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
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = terrainSpriteConfiguration.GetSprite(World.GetSquare(position));
        // Apply the tile to the tilemap
        terrainTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
    }

    /// <summary>
    /// To be called whenever a structure is updated at the specified position
    /// </summary>
    /// <param name="position">The world position as a Vector2</param>
    private void StructureUpdated(Vector2Int position)
    {
        // If a structure is removed then set the structure tile to null
        if (World.GetSquare(position).InstalledStructure == null)
        {
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        // If a structure is added then find the correct tile from the configuration
        else
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = structureSpriteConfiguration.GetSprite(World.GetSquare(position).InstalledStructure);
            structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
        // List of squares to be updated in order to ensure neighbours are correct
        List<Square> squares = new List<Square>();
        if (World.GetSquare(position).SquareNorth != null) squares.Add(World.GetSquare(position).SquareNorth);
        if (World.GetSquare(position).SquareEast != null) squares.Add(World.GetSquare(position).SquareEast);
        if (World.GetSquare(position).SquareSouth != null) squares.Add(World.GetSquare(position).SquareSouth);
        if (World.GetSquare(position).SquareWest != null) squares.Add(World.GetSquare(position).SquareWest);
        // Loop through and update the sprites
        foreach (Square square in squares)
        {
            if (square != null && square.InstalledStructure != null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = structureSpriteConfiguration.GetSprite(square.InstalledStructure);
                structureTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(square.Position.x, square.Position.y, 0), tile);
            }
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
        Vector3 position = WorldGrid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, World.Size.x);
        position.y = Mathf.Clamp(position.y, 0, World.Size.y);
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
