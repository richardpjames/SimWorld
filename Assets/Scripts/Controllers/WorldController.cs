using UnityEngine;

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
}
