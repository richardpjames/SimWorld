using UnityEngine;
using UnityEngine.Tilemaps;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }
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
        // Create a new pool with the initial number of agents
    }

    // These are all of the possible tiles and items in the game, available to instantiate to other classes
    [Header("Terrain")]
    [SerializeField] private TileBase _grassTile;
    public Terrain Grass { get => new Terrain(name: "Grass", tile: _grassTile, movementCost: 1, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Grass); }
    [SerializeField] private TileBase _sandTile;
    public Terrain Sand { get => new Terrain(name: "Sand", tile: _sandTile, movementCost: 0.5f, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Sand); }
    [SerializeField] private TileBase _waterTile;
    public Terrain Water { get => new Terrain(name: "Water", tile: _waterTile, movementCost: 0, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Water, buildingAllowed: false); }
    // Structures start here
    [Header("Structures")]
    [SerializeField] private TileBase _woodenWallTile;
    public Wall WoodenWall { get => new Wall(name: "Wooden Wall", tile: _woodenWallTile, movementCost: 1, width: 1, height: 1, buildCost: 1); }
    [SerializeField] private TileBase _woodenDoorTile;
    public Door WoodenDoor { get => new Door(name: "Wooden Door", tile: _woodenDoorTile, movementCost: 1, width: 1, height: 1, buildCost: 1); }
    // Floors start here
    [Header("Floors")]
    [SerializeField] private TileBase _woodenFloorTile;
    public Floor WoodenFloor { get => new Floor(name: "Wooden Floor", tile: _woodenFloorTile, movementCost: 1, width: 1, height: 1, buildCost: 1); }
    [SerializeField] private TileBase _footpathTile;
    public Floor StoneFootpath { get => new Floor(name: "Stone Footpath", tile: _footpathTile, movementCost: 1, width: 1, height: 1, buildCost: 1); }
}