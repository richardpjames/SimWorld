using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class GraphicsController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Grid _worldGrid;
    [SerializeField] private Tilemap _terrainTilemap;
    [SerializeField] private Tilemap _floorTilemap;
    [SerializeField] private Tilemap _structureTilemap;
    [SerializeField] private Tilemap _floorJobTilemap;
    [SerializeField] private Tilemap _structureJobTilemap;
    [SerializeField] private Tilemap _demolitionJobTilemap;
    [SerializeField] private TileBase _demolitionTile;
    [Header("Agents")]
    [SerializeField] private GameObject _agentPrefab;
    private Dictionary<Agent, GameObject> _agents;

    // Accessors for easier access to controllers etc.
    private World _world { get => WorldController.Instance.World; }
    private AgentPool _agentPool { get => AgentController.Instance.AgentPool; }
    private JobQueue _jobQueue { get =>JobController.Instance.JobQueue; }


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
        // Initialise the agents dictionary
        _agents = new Dictionary<Agent, GameObject>();
    }

    private void Start()
    {
        // Subscribe to any events from the world
        _world.OnSquareUpdated += OnSquareUpdated;
        _jobQueue.OnJobCreated += OnJobCreated;
        _jobQueue.OnJobCompleted += OnJobCompleted;
        // Subscribe to any events about agents
        _agentPool.OnAgentCreated += OnAgentCreated;
        _agentPool.OnAgentUpdated += OnAgentUpdated;
        // Initial draw of all tiles
        RedrawTiles();
        // Initial creation of all agents
        RedrawAgents();
    }

    /// <summary>
    /// Adds new agents to the world as game objects
    /// </summary>
    /// <param name="agent"></param>
    private void OnAgentCreated(Agent agent)
    {
        // Create a new game object and set this as the parent
        GameObject newGameObject = Instantiate(_agentPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        newGameObject.transform.SetParent(transform, true);
        // Add to the list of agents
        _agents.Add(agent, newGameObject);
    }

    /// <summary>
    /// Called after any agent is updated
    /// </summary>
    /// <param name="agent"></param>
    private void OnAgentUpdated(Agent agent)
    {
        // Update the lcoation of the game object based on the data
        _agents[agent].transform.position = new Vector3(agent.Position.x, agent.Position.y, _agents[agent].transform.position.z);
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
        GetTilemap(JobType.Floor).ClearAllTiles();
        GetTilemap(JobType.Structure).ClearAllTiles();
        GetTilemap(JobType.Demolish).ClearAllTiles();
    }

    /// <summary>
    /// Redraws all tiles (for use after world initialisation)
    /// </summary>
    public void RedrawTiles()
    {
        // First remove any existing tiles
        ClearTiles();
        // Loop over the whole world and redraw the tiles
        for (int x = 0; x < _world.Size.x; x++)
        {
            for (int y = 0; y < _world.Size.y; y++)
            {
                OnSquareUpdated(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// Redraws all agents into the scene, used at initialisation
    /// </summary>
    public void RedrawAgents()
    {
        // Clear any existing agents
        ClearAgents();
        // Trigger the agent created
        foreach (Agent agent in _agentPool.Agents)
        {
            OnAgentCreated(agent);
        }
    }

    /// <summary>
    /// Removes all agents from the scene
    /// </summary>
    public void ClearAgents()
    {
        if (_agents != null)
        {
            // Destroy all game objects in the dictionary
            foreach (GameObject gameObject in _agents.Values)
            {
                Destroy(gameObject);
            }
            // Set to a new dictionary
            _agents = new Dictionary<Agent, GameObject>();
        }
    }

    /// <summary>
    /// Update the graphics whenever a job is created
    /// </summary>
    /// <param name="job"></param>
    private void OnJobCreated(Job job)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetTilemap(job.Type);
        // If there is a tilemap and tile then update 
        if (job.Type == JobType.Demolish && tilemap != null)
        {
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), _demolitionTile);
        }
        else if (tilemap != null && job.Indicator != null)
        {
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), job.Indicator);
        }
    }

    /// <summary>
    /// Update the graphics whenever a job is completed
    /// </summary>
    /// <param name="job"></param>
    private void OnJobCompleted(Job job)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetTilemap(job.Type);
        if (tilemap != null)
        {
            // Remove the indicator
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), null);
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
        // If present - update the terrain otherwise set to null
        if (terrain != null) GetTilemap<Terrain>().SetTile(new Vector3Int(position.x, position.y, 0), terrain.Tile);
        else GetTilemap<Terrain>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        // If present - update the floors otherwise set to null
        if (floor != null) GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), floor.Tile);
        else GetTilemap<Floor>().SetTile(new Vector3Int(position.x, position.y, 0), null);
        // If present - update the structures otherwise set to null
        if (structure != null) GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), structure.Tile);
        else GetTilemap<Structure>().SetTile(new Vector3Int(position.x, position.y, 0), null);
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
        if (_world == null || _worldGrid == null)
        {
            return Vector2Int.zero;
        }
        // Get the correct position from the grid
        Vector3 position = _worldGrid.WorldToCell(new Vector3(x, y, 0));
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
        if (typeof(T) == typeof(Structure)) return _structureTilemap.GetComponent<Tilemap>();
        if (typeof(T) == typeof(Floor)) return _floorTilemap.GetComponent<Tilemap>();
        if (typeof(T) == typeof(Terrain)) return _terrainTilemap.GetComponent<Tilemap>();
        // For all other types return null
        return null;
    }
    /// <summary>
    /// Gets the tilemap for the provided job target (for indicators)
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private Tilemap GetTilemap(JobType target)
    {
        if (target == JobType.Structure) return _structureJobTilemap.GetComponent<Tilemap>();
        if (target == JobType.Floor) return _floorJobTilemap.GetComponent<Tilemap>();
        if (target == JobType.Demolish) return _demolitionJobTilemap.GetComponent<Tilemap>();
        // For all other types return null
        return null;
    }
}
