using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ConstructionController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private StructureDataConfiguration structureDataConfiguration;
    [SerializeField] private FloorDataConfiguration floorDataConfiguration;

    // For tracking what we are currently building
    private TerrainType currentTerrainType = TerrainType.Grass;
    private StructureType currentStructureType = StructureType.Wall;
    private FloorType currentFloorType = FloorType.Wooden;
    // For tracking the type of construction we are doing
    private BuildMode currentBuildMode = BuildMode.Terrain;

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
    }

    /// <summary>
    /// Builds based on current configuration once a drag is complete
    /// </summary>
    private void Build(Vector2Int topLeft, Vector2Int bottomRight)
    {
        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = topLeft.y; y <= bottomRight.y; y++)
            {
                if (currentBuildMode == BuildMode.Terrain)
                {
                    WorldController.Instance.SetTerrainType(new Vector2Int(x, y), currentTerrainType);
                }
                else if (currentBuildMode == BuildMode.Structure)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    StructureDataConfiguration.StructureConfiguration config = structureDataConfiguration.GetConfiguration(currentStructureType);
                    // Build a structure from that configuration
                    Structure structure = new Structure(config);
                    // Place it into the world
                    WorldController.Instance.InstallStructure(new Vector2Int(x, y), structure);
                }
                else if (currentBuildMode == BuildMode.Floor)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    FloorDataConfiguration.FloorConfiguration config = floorDataConfiguration.GetConfiguration(currentFloorType);
                    // Build a structure from that configuration
                    Floor floor = new Floor(config);
                    // Place it into the world
                    WorldController.Instance.InstallFloor(new Vector2Int(x, y), floor);
                }
                else if (currentBuildMode == BuildMode.Demolish)
                {
                    // During demolision we first look for any structures (and remove) and then next, any floors
                    if (WorldController.Instance.GetStructure(new Vector2Int(x, y)) != null)
                    {
                        WorldController.Instance.RemoveStructure(new Vector2Int(x, y));
                    }
                    else if (WorldController.Instance.GetFloor(new Vector2Int(x, y)) != null)
                    {
                        WorldController.Instance.RemoveFloor(new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set the construction mode to placing terrain as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetTerrain(TerrainType type)
    {
        currentBuildMode = BuildMode.Terrain;
        currentTerrainType = type;
    }
    /// <summary>
    /// Set the construction mode to placing structures as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetStucture(StructureType type)
    {
        currentBuildMode = BuildMode.Structure;
        currentStructureType = type;
    }
    /// <summary>
    /// Set the construction mode to placing floors as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetFloor(FloorType type)
    {
        currentBuildMode = BuildMode.Floor;
        currentFloorType = type;
    }
    /// <summary>
    /// Set the construction mode to demolish structures (and then floors)
    /// </summary>
    public void SetDemolish()
    {
        currentBuildMode = BuildMode.Demolish;
    }
}
