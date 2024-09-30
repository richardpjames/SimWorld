using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;


public class WorldController : MonoBehaviour
{
    [Header("World Configuration")]
    [SerializeField] private TerrainDataConfiguration terrainDataConfiguration;
    [Header("Biome Generation")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;

    // All access to the world is through the controller (no direct references)
    public World World { get; private set; }
    // Access to jobs
    Job currentJob = null;

    // Accessor to make things easier
    private GameController _game { get => GameController.Instance; }

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // On load initialize the world
            Initialize(_game.WorldName, new Vector2Int(_game.WorldWidth, _game.WorldHeight));
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
        // Initialise a new world
        World = new World(name, size, terrainDataConfiguration);
        // Generate random biomes
        World.GenerateBiomes(waves, scale, terrainDataConfiguration);

        // Set the correct tile in the tilemap for each square
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
            {
                // Create a tile with the correct sprite and add to the tilemap
                Terrain terrain = World.Get<Terrain>(new Vector2Int(x, y));
                TileBase tile = terrain.Tile;
            }
        }

        // Set the camera to the center of the world
        Camera.main.transform.position = new Vector3(World.Size.x / 2, World.Size.y / 2, Camera.main.transform.position.z);
    }

    private void Update()
    {
        if ((World != null && World.JobQueue.Count > 0) && (currentJob == null || currentJob.Complete))
        {
            // Get the next job from the list
            currentJob = World.JobQueue.Dequeue();
        }
        // Progress that job
        if (currentJob != null)
        {
            currentJob.Work(Time.deltaTime * 4);
        }
    }

    /// <summary>
    /// Installs a structure or floor at the specified position and also updates the graphics.
    /// </summary>
    public void Install<T>(Vector2Int position, T item) where T : TileType, IBuildableObject
    {
        // Create the job and subscribe to the completion
        Job job = World.Install<T>(position, item, 1f);
    }

    /// <summary>
    /// Removes a floor or structure at the specified position.
    /// </summary>
    public void Remove<T>(Vector2Int position) where T : TileType, IBuildableObject
    {
        Job job = World.Remove<T>(position, 1f);
    }

    /// <summary>
    /// Gets the structure, floor or terrain at the specified position.
    /// </summary>
    public T Get<T>(Vector2Int position) where T : TileType
    {
        return World.Get<T>(position);
    }
}
