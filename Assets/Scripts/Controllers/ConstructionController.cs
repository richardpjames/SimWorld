using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
        MouseController.Instance.OnDragComplete += Build;
        MouseController.Instance.OnDeselectComplete += () => { _currentBuildMode = BuildMode.None; OnBuildingModeSet?.Invoke(null); };
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
                if (_currentBuildMode == BuildMode.Structure)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    StructureDataConfiguration.StructureConfiguration config = structureDataConfiguration.GetConfiguration(_currentStructureType);
                    // Build a structure from that configuration
                    Structure structure = new Structure(config);
                    // Place it into the world
                    WorldController.Instance.Install<Structure>(new Vector2Int(x, y), structure);
                }
                else if (_currentBuildMode == BuildMode.Floor)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    FloorDataConfiguration.FloorConfiguration config = floorDataConfiguration.GetConfiguration(_currentFloorType);
                    // Build a structure from that configuration
                    Floor floor = new Floor(config);
                    // Place it into the world
                    WorldController.Instance.Install(new Vector2Int(x, y), floor);
                }
                else if (_currentBuildMode == BuildMode.Demolish)
                {
                    // During demolision we first look for any structures (and remove) and then next, any floors
                    if (WorldController.Instance.Get<Structure>(new Vector2Int(x, y)) != null)
                    {
                        WorldController.Instance.Remove<Structure>(new Vector2Int(x, y));
                    }
                    else if (WorldController.Instance.Get<Floor>(new Vector2Int(x, y)) != null)
                    {
                        WorldController.Instance.Remove<Floor>(new Vector2Int(x, y));
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
