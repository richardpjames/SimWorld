using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private StructureDataConfiguration structureDataConfiguration;
    [SerializeField] private FloorDataConfiguration floorDataConfiguration;
    [SerializeField] private TileBase indicatorTile;

    // For tracking what we are currently building
    private StructureType _currentStructureType = StructureType.Wall;
    private FloorType _currentFloorType = FloorType.Wooden;
    // For tracking the type of construction we are doing
    private BuildMode _currentBuildMode = BuildMode.None;

    // Actions
    public Action<TileBase> OnBuildingModeSet;

    // Accessors for easier access
    private World _world { get => WorldController.Instance.World; }
    private MouseController _mouse { get => MouseController.Instance; }
    private JobQueue _jobQueue { get => JobController.Instance.JobQueue; }

    // Allow for singleton pattern
    public static ConstructionController Instance { get; private set; }

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
        _mouse.OnDragComplete += Build;
        _mouse.OnDeselectComplete += () => { _currentBuildMode = BuildMode.None; OnBuildingModeSet?.Invoke(null); };
    }

    /// <summary>
    /// Builds based on current configuration once a drag is complete
    /// </summary>
    private void Build(Vector2Int topLeft, Vector2Int bottomRight)
    {
        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = bottomRight.y; y >= topLeft.y; y--)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (_currentBuildMode == BuildMode.Structure)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    StructureDataConfiguration.StructureConfiguration config = structureDataConfiguration.GetConfiguration(_currentStructureType);
                    // Build a structure from that configuration
                    Structure structure = new Structure(config);
                    // Place it into the world
                    _jobQueue.Add(new Job(position, (job) => { _world.Install<Structure>(position, structure); }, 1f, structure.Tile, JobType.Structure));
                }
                else if (_currentBuildMode == BuildMode.Floor)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    FloorDataConfiguration.FloorConfiguration config = floorDataConfiguration.GetConfiguration(_currentFloorType);
                    // Build a structure from that configuration
                    Floor floor = new Floor(config);
                    // Place it into the world
                    _jobQueue.Add(new Job(position, (job) => { _world.Install<Floor>(position, floor); }, 1f, floor.Tile, JobType.Floor));
                }
                else if (_currentBuildMode == BuildMode.Demolish)
                {
                    // During demolision we first look for any structures (and remove) and then next, any floors
                    if (_world.Get<Structure>(position) != null)
                    {
                        _jobQueue.Add(new Job(position, (job) => { _world.Remove<Structure>(position); }, 1f, indicatorTile, JobType.Demolish));
                    }
                    else if (_world.Get<Floor>(position) != null)
                    {
                        _jobQueue.Add(new Job(position, (job) => { _world.Remove<Floor>(position); }, 1f, indicatorTile, JobType.Demolish));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set the construction mode to placing structures as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetStucture(StructureType type)
    {
        _currentBuildMode = BuildMode.Structure;
        _currentStructureType = type;
        OnBuildingModeSet?.Invoke(structureDataConfiguration.GetTile(type));
    }
    /// <summary>
    /// Set the construction mode to placing floors as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetFloor(FloorType type)
    {
        _currentBuildMode = BuildMode.Floor;
        _currentFloorType = type;
        OnBuildingModeSet?.Invoke(floorDataConfiguration.GetTile(type));
    }
    /// <summary>
    /// Set the construction mode to demolish structures (and then floors)
    /// </summary>
    public void SetDemolish()
    {
        _currentBuildMode = BuildMode.Demolish;
        OnBuildingModeSet?.Invoke(indicatorTile);
    }
}
