using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class GraphicsController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap structureTilemap;
    [SerializeField] private Tilemap floorJobTilemap;
    [SerializeField] private Tilemap structureJobTilemap;
    [SerializeField] private Tilemap demolitionJobTilemap;
    [SerializeField] private TileBase demolitionTile;

    // Accessors for easier access to controllers etc.
    private World _world { get => WorldController.Instance.World; }

    // Allow for a static instance and accessible variables
    public static GraphicsController Instance { get; private set; }

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

    private void Start()
    {
        // Clear any existing tiles from the tilemap
        ClearTiles();
        // Subscribe to any events from the world
        _world.OnSquareUpdated += OnSquareUpdated;
        _world.OnJobCreated += OnJobCreated;
        _world.OnJobCompleted += OnJobCompleted;
        // Initialize by drawing all tiles
        for (int x = 0; x < _world.Size.x; x++)
        {
            for (int y = 0; y < _world.Size.y; y++)
            {
                OnSquareUpdated(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// Remove any tiles from the tilemaps
    /// </summary>
    private void ClearTiles()
    {
        // Clear any tiles if there is a tilemap present
        GetTilemap<Terrain>().ClearAllTiles();
        GetTilemap<Structure>().ClearAllTiles();
        GetTilemap<Floor>().ClearAllTiles();
        GetJobTilemap(JobTarget.Floor).ClearAllTiles();
        GetJobTilemap(JobTarget.Structure).ClearAllTiles();
        GetJobTilemap(JobTarget.Demolish).ClearAllTiles();
    }

    /// <summary>
    /// Update the graphics whenever a job is created
    /// </summary>
    /// <param name="job"></param>
    private void OnJobCreated(Job job, TileType tile)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetJobTilemap(job.Target);
        // If there is a tilemap and tile then update 
        if (job.Target == JobTarget.Demolish && tilemap != null)
        {
            tilemap.SetTile(new Vector3Int(job.Location.x, job.Location.y, 0), demolitionTile);
        }
        else if (tilemap != null && tile != null)
        {
            tilemap.SetTile(new Vector3Int(job.Location.x, job.Location.y, 0), tile.Tile);
        }
    }

    /// <summary>
    /// Update the graphics whenever a job is completed
    /// </summary>
    /// <param name="job"></param>
    private void OnJobCompleted(Job job)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetJobTilemap(job.Target);
        if (tilemap != null)
        {
            // Remove the indicator
            tilemap.SetTile(new Vector3Int(job.Location.x, job.Location.y, 0), null);
        }
    }

    /// <summary>
    /// Update the tilemap whenever a square is updated at a particular position
    /// </summary>
    /// <param name="position"></param>
    private void OnSquareUpdated(Vector2Int position)
    {
        // Get the terrain, floors and stuctures at the position
        Terrain terrain = _world.Get<Terrain>(position);
        Floor floor = _world.Get<Floor>(position);
        Structure structure = _world.Get<Structure>(position);
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
    /// Takes a world position and determines which tile occupies that position (with an x, y coordinate returned).
    /// </summary>
    /// <param name="x">The x world position</param>
    /// <param name="y">The y world position</param>
    /// <returns>The position of the tile which occupies the requested position</returns>
    public Vector2Int GetSquarePosition(float x, float y)
    {
        // Check that the world is created
        if (_world == null || worldGrid == null)
        {
            return Vector2Int.zero;
        }
        // Get the correct position from the grid
        Vector3 position = worldGrid.WorldToCell(new Vector3(x, y, 0));
        // Clamp to the world bounds
        position.x = Mathf.Clamp(position.x, 0, _world.Size.x - 1);
        position.y = Mathf.Clamp(position.y, 0, _world.Size.y - 1);
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
        // For all other types return null
        return null;
    }
    /// <summary>
    /// Gets the tilemap for the provided job target (for indicators)
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private Tilemap GetJobTilemap(JobTarget target)
    {
        if (target == JobTarget.Structure) return structureJobTilemap.GetComponent<Tilemap>();
        if (target == JobTarget.Floor) return floorJobTilemap.GetComponent<Tilemap>();
        if (target == JobTarget.Demolish) return demolitionJobTilemap.GetComponent<Tilemap>();
        // For all other types return null
        return null;
    }
}
