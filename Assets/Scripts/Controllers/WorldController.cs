using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;


public class WorldController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap structureTilemap;
    [SerializeField] private Tilemap jobTilemap;
    [SerializeField] private TileBase demolisionTile;
    [Header("World Configuration")]
    [SerializeField] private TerrainDataConfiguration terrainDataConfiguration;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    // All access to the world is through the controller (no direct references)
    private World world;

    // Access to jobs
    Job currentJob = null;

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
        GetTilemap<Job>().ClearAllTiles();

        // Initialise a new world
        world = new World(name, size, terrainDataConfiguration);
        world.OnSquareUpdated += UpdateSquare;
        // Generate random biomes
        world.GenerateBiomes(waves, scale, terrainDataConfiguration);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < world.Size.x; x++)
        {
            for (int y = 0; y < world.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                Terrain terrain = world.Get<Terrain>(new Vector2Int(x, y));
                TileBase tile = terrain.Tile;
                GetTilemap<Terrain>().SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(world.Size.x / 2, world.Size.y / 2, Camera.main.transform.position.z);
    }

    private void Update()
    {
        if ((world != null && world.JobQueue.Count > 0) && (currentJob == null || currentJob.Complete))
        {
            // Get the next job from the list
            currentJob = world.JobQueue.Dequeue();
        }
        // Progress that job
        if (currentJob != null)
        {
            currentJob.Work(Time.deltaTime * 4);
        }
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

    private void UpdateSquare(Vector2Int position)
    {
        // Get the terrain, floors and stuctures at the position
        Terrain terrain = world.Get<Terrain>(position);
        Floor floor = world.Get<Floor>(position);
        Structure structure = world.Get<Structure>(position);
        // Update the tilemaps - if there is a type of tile present then update the tilemap, otherwise show null
        if (terrain != null)
        {
            GetTilemap<Terrain>().SetTile(new Vector3Int(position.x, position.y, 0), terrain.Tile);
        }
        else
        {
            GetTilemap<Terrain>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        if (floor != null)
        {
            GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), floor.Tile);
        }
        else
        {
            GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        if (structure != null)
        {
            GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), structure.Tile);
        }
        else
        {
            GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
    }

    /// <summary>
    /// Updated graphics when a job is complete.
    /// </summary>
    /// <param name="position"></param>
    private void JobComplete(Vector2Int position)
    {
        // When a job is complete at a position, then remove the tile
        Tilemap tilemap = GetTilemap<Job>();
        tilemap.SetTile(new Vector3Int(position.x, position.y, 0), null);
    }

    /// <summary>
    /// Installs a structure or floor at the specified position and also updates the graphics.
    /// </summary>
    public void Install<T>(Vector2Int position, T item) where T : TileType, IBuildableObject
    {
        // Create the job and subscribe to the completion
        Job job = world.Install<T>(position, item, 1f);
        if (job != null)
        {
            job.OnJobComplete += JobComplete;
            // Get the jobs tilemap (which will display the temporary indicator
            Tilemap tilemap = GetTilemap<Job>();
            tilemap.SetTile(new Vector3Int(position.x, position.y, 0), item.Tile);
        }
    }

    /// <summary>
    /// Removes a floor or structure at the specified position.
    /// </summary>
    public void Remove<T>(Vector2Int position) where T : TileType, IBuildableObject
    {
        Job job = world.Remove<T>(position, 1f);
        if (job != null)
        {
            job.OnJobComplete += JobComplete;
            // Get the jobs tilemap (which will display the temporary indicator
            Tilemap tilemap = GetTilemap<Job>();
            tilemap.SetTile(new Vector3Int(position.x, position.y, 0), demolisionTile);
        }
    }

    /// <summary>
    /// Gets the structure, floor or terrain at the specified position.
    /// </summary>
    public T Get<T>(Vector2Int position) where T : TileType
    {
        return world.Get<T>(position);
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
        if (typeof(T) == typeof(Job)) return jobTilemap.GetComponent<Tilemap>();
        // For all other types return null
        return null;
    }

}
