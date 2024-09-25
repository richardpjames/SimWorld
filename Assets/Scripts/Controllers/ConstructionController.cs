using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ConstructionController : MonoBehaviour
{
    [Header("Structure Configuration")]
    [SerializeField] private StructureDataConfiguration structureDataConfiguration;
    // For tracking what we are currently building
    private TerrainType currentTerrainType = TerrainType.Grass;
    private StructureType currentStructureType = StructureType.Wall;
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
                    WorldController.Instance.SetSquareType(new Vector2Int(x, y), currentTerrainType);
                }
                else if (currentBuildMode == BuildMode.Structure)
                {
                    // Get the configuration for the currently selected type from a scriptable object
                    StructureDataConfiguration.StructureConfiguration config = structureDataConfiguration.GetConfiguration(currentStructureType);
                    // Build a structure from that configuration
                    Structure structure = new Structure(config);
                    // Place it into the world
                    WorldController.Instance.World.GetSquare(new Vector2Int(x, y)).InstallStructure(structure);
                }
                else if (currentBuildMode == BuildMode.Demolish)
                {
                    WorldController.Instance.World.GetSquare(new Vector2Int(x, y)).RemoveStructure();
                }
            }
        }
    }

    /// <summary>
    /// Set the construction mode to placing terrain as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetTerrain(string name)
    {
        currentBuildMode = BuildMode.Terrain;
        Enum.TryParse(name, out currentTerrainType);
    }
    /// <summary>
    /// Set the construction mode to placing structures as specified by the named enum
    /// </summary>
    /// <param name="name">The name for the enum (which will be parsed)</param>
    public void SetStucture(string name)
    {
        currentBuildMode = BuildMode.Structure;
        Enum.TryParse(name, out currentStructureType);
    }
    /// <summary>
    /// Set the construction mode to demolish structures
    /// </summary>
    public void SetDemolish()
    {
        currentBuildMode = BuildMode.Demolish;
    }
}
