using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GraphicsManager : MonoBehaviour
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
    private World _world { get => WorldManager.Instance.World; }
    private AgentPool _agentPool { get => AgentManager.Instance.AgentPool; }
    private JobQueue _jobQueue { get => JobManager.Instance.JobQueue; }


    // Allow for a static instance and accessible variables
    public static GraphicsManager Instance { get; private set; }

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
        _world.OnTileUpdated += OnTileUpdated;
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
        GameObject newGameObject = Instantiate(_agentPrefab, agent.Position, Quaternion.identity);
        newGameObject.transform.SetParent(transform, true);
        // Add to the list of agents
        _agents.Add(agent, newGameObject);
    }

    private void OnAgentUpdated(Agent agent)
    {
        // Update the lcoation of the game object based on the data
        _agents[agent].transform.position = new Vector3(agent.Position.x, agent.Position.y, _agents[agent].transform.position.z);
    }

    private void ClearTiles()
    {
        // Clear any tiles if there is a tilemap present
        _terrainTilemap.GetComponent<Tilemap>().ClearAllTiles();
        _floorTilemap.GetComponent<Tilemap>().ClearAllTiles();
        _structureTilemap.GetComponent<Tilemap>().ClearAllTiles();
        _floorJobTilemap.GetComponent<Tilemap>().ClearAllTiles();
        _structureJobTilemap.GetComponent<Tilemap>().ClearAllTiles();
        _demolitionJobTilemap.GetComponent<Tilemap>().ClearAllTiles();
    }

    public void RedrawTiles()
    {
        // First remove any existing tiles
        ClearTiles();
        // Loop over the whole world and redraw the tiles
        for (int x = 0; x < _world.Size.x; x++)
        {
            for (int y = 0; y < _world.Size.y; y++)
            {
                OnTileUpdated(new Vector2Int(x, y));
            }
        }
    }

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

    private void OnJobCreated(Job job)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetJobTilemap(job.Layer);
        // If there is a tilemap and tile then update 
        if (job.GetType() == typeof(DemolishJob))
        {
            tilemap = _demolitionJobTilemap.GetComponent<Tilemap>();
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), _demolitionTile);
        }
        //else 
        else if (tilemap != null && job.Indicator != null)
        {
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), job.Indicator);
        }
    }

    private void OnJobCompleted(Job job)
    {
        // Get the correct tilemap for this type of job
        Tilemap tilemap = GetJobTilemap(job.Layer);
        // Check for demolition in particular
        if (job.GetType() == typeof(DemolishJob))
        {
            tilemap = _demolitionJobTilemap.GetComponent<Tilemap>();
        }
        // If we have found the tilemap then remove it
        if (tilemap != null)
        {
            // Remove the indicator
            tilemap.SetTile(new Vector3Int(job.Position.x, job.Position.y, 0), null);
        }
    }

    private void OnTileUpdated(Vector2Int position)
    {
        // Get the terrain, floors and stuctures at the position
        WorldTile terrain = _world.GetWorldTile(position, WorldLayer.Terrain);
        WorldTile floor = _world.GetWorldTile(position, WorldLayer.Floor);
        WorldTile structure = _world.GetWorldTile(position, WorldLayer.Structure);
        // If present - update the terrain otherwise set to null
        if (terrain != null) GetTilemap(WorldLayer.Terrain).SetTile(new Vector3Int(position.x, position.y, 0), terrain.Tile);
        else GetTilemap(WorldLayer.Terrain).SetTile(new Vector3Int(position.x, position.y, 0), null);
        // If present - update the floors otherwise set to null
        if (floor != null) GetTilemap(WorldLayer.Floor).SetTile(new Vector3Int(position.x, position.y, 0), floor.Tile);
        else GetTilemap(WorldLayer.Floor).SetTile(new Vector3Int(position.x, position.y, 0), null);
        // If present - update the structures otherwise set to null
        if (structure != null) GetTilemap(WorldLayer.Structure).SetTile(new Vector3Int(position.x, position.y, 0), structure.Tile);
        else GetTilemap(WorldLayer.Structure).SetTile(new Vector3Int(position.x, position.y, 0), null);
    }

    public Vector2Int GetTilePosition(float x, float y)
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

    private Tilemap GetJobTilemap(WorldLayer layer)
    {
        if (layer == WorldLayer.Structure) return _structureJobTilemap.GetComponent<Tilemap>();
        if (layer == WorldLayer.Floor) return _floorJobTilemap.GetComponent<Tilemap>();
        // Otherwise return null
        return null;
    }

    private Tilemap GetTilemap(WorldLayer layer)
    {
        // Get the correct tilemap based on the structures layer
        if (layer == WorldLayer.Terrain) return _terrainTilemap.GetComponent<Tilemap>();
        if (layer == WorldLayer.Structure) return _structureTilemap.GetComponent<Tilemap>();
        if (layer == WorldLayer.Floor) return _floorTilemap.GetComponent<Tilemap>();
        // Otherwise return null
        return null;
    }
}
