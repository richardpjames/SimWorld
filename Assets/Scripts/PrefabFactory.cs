using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PrefabFactory : MonoBehaviour
{
    [Header("Terrain")]
    [SerializeField] private TileBase _grassTile;
    [SerializeField] private TileBase _sandTile;
    [SerializeField] private TileBase _waterTile;
    [Header("Structures")]
    [SerializeField] private TileBase _woodenWallTile;
    [SerializeField] private TileBase _woodenDoorTile;
    [SerializeField] private TileBase _bedTile;
    [SerializeField] private TileBase _saplingTile;
    [SerializeField] private TileBase _treeTile;
    [SerializeField] private TileBase _rockTile;
    [SerializeField] private TileBase _woodcuttersTile;
    [SerializeField] private TileBase _stonecuttersTile;
    [SerializeField] private TileBase _carpentersTile;
    [SerializeField] private TileBase _stonemasonsTile;
    [Header("Floors")]
    [SerializeField] private TileBase _woodenFloorTile;
    [SerializeField] private TileBase _stoneFloorTile;
    private JobQueue _queue;
    private World _world;
    private Inventory _inventory;

    private Dictionary<string, WorldTile> _prefabs;

    private void Awake()
    {
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _world = GameObject.FindAnyObjectByType<World>();
        _queue = GameObject.FindAnyObjectByType<JobQueue>();
        // List of all prefabs
        _prefabs = new Dictionary<string, WorldTile>();
        // WorldTile reference to re-use
        WorldTile tile;

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Grass,
            name: "Grass", tile: _grassTile);
        _prefabs.Add(tile.Name, tile);

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Sand,
            name: "Sand", movementCost: 0.5f, tile: _sandTile);
        _prefabs.Add(tile.Name, tile);

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Water,
            name: "Water", movementCost: 0, tile: _waterTile, buildingAllowed: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenWallCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 2 } };
        tile = new WorldTile(type: TileType.Wall, buildMode: BuildMode.Line, layer: WorldLayer.Structure,
            buildTime: 1, name: "Wooden Wall", movementCost: 0, tile: _woodenWallTile, cost: woodenWallCost, yield: woodenWallCost);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenDoorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 3 } };
        tile = new WorldTile(type: TileType.Door, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            buildTime: 1, name: "Wooden Door", tile: _woodenDoorTile, cost: woodenDoorCost, yield: woodenDoorCost);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenBedCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 10 } };
        tile = new WorldTile(type: TileType.Bed, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: -2, buildTime: 3, name: "Wooden Bed", movementCost: 0, tile: _bedTile,
            cost: woodenBedCost, yield: woodenBedCost, canRotate: true);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodcuttersTableCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 10 } };
        tile = new WorldTile(type: TileType.HarvestersTable, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 3, height: -2, buildTime: 3, name: "Woodcutters Table", movementCost: 0, tile: _woodcuttersTile,
            cost: woodcuttersTableCost, yield: woodcuttersTableCost, canRotate: true, requiresUpdate: true,
            harvestType: TileType.Tree);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> stonecuttersTableCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 10 } };
        tile = new WorldTile(type: TileType.HarvestersTable, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 3, height: -2, buildTime: 3, name: "Stonecutters Table", movementCost: 0, tile: _stonecuttersTile,
            cost: stonecuttersTableCost, yield: stonecuttersTableCost, canRotate: true, requiresUpdate: true,
            harvestType: TileType.Rock);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> stonemasonsTableCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 50 }, { InventoryItem.Stone, 50 } };
        Dictionary<InventoryItem, int> stonemasonsCraftCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Stone, 3 } };
        Dictionary<InventoryItem, int> stonemasonsCraftYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Blocks, 1 } };
        tile = new WorldTile(type: TileType.CraftersTable, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 3, height: -2, buildTime: 5, name: "Stonemasons Table", movementCost: 0, tile: _stonemasonsTile,
            cost: stonemasonsTableCost, yield: stonemasonsTableCost, canRotate: true, requiresUpdate: true,
            craftCost: stonemasonsCraftCost, craftYield: stonemasonsCraftYield, craftTime: 5);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> carpentersTableCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 50 }, { InventoryItem.Stone, 50 } };
        Dictionary<InventoryItem, int> carpentersCraftCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 3 } };
        Dictionary<InventoryItem, int> carpentersCraftYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Planks, 1 } };
        tile = new WorldTile(type: TileType.CraftersTable, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 3, height: -2, buildTime: 5, name: "Carpenters Table", movementCost: 0, tile: _carpentersTile,
            cost: carpentersTableCost, yield: carpentersTableCost, canRotate: true, requiresUpdate: true,
            craftCost: carpentersCraftCost, craftYield: carpentersCraftYield, craftTime: 5);
        _prefabs.Add(tile.Name, tile);


        Dictionary<InventoryItem, int> treeYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 10 }, { InventoryItem.Seeds, 5 } };
        tile = new WorldTile(type: TileType.Tree, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: 2, buildTime: 5, name: "Tree", movementCost: 0, tile: _treeTile,
            buildingAllowed: false, yield: treeYield);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> saplingCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Seeds, 1 } };
        WorldTile sapling = new WorldTile(type: TileType.Sapling, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: 2, buildTime: 1, name: "Sapling", movementCost: 0, tile: _saplingTile, requiresUpdate: true,
            buildingAllowed: false, cost: saplingCost, growthTime: 25, adultTile: tile);
        _prefabs.Add(sapling.Name, sapling);

        Dictionary<InventoryItem, int> rockYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Stone, 2 } };
        tile = new WorldTile(type: TileType.Rock, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            buildTime: 1, name: "Rock", movementCost: 0, tile: _rockTile, buildingAllowed: false, yield: rockYield);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenFloorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 2 } };
        tile = new WorldTile(type: TileType.Floor, buildMode: BuildMode.Drag, layer: WorldLayer.Floor,
            buildTime: 1, name: "Wooden Floor", tile: _woodenFloorTile, cost: woodenFloorCost, yield: woodenFloorCost);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> stoneFloorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Stone, 2 } };
        tile = new WorldTile(type: TileType.Floor, buildMode: BuildMode.Drag, layer: WorldLayer.Floor,
            width: 1, height: 1, buildTime: 1, name: "Stone Floor", tile: _stoneFloorTile, cost: stoneFloorCost, yield: stoneFloorCost);
        _prefabs.Add(tile.Name, tile);
    }

    public WorldTile Create(string name)
    {
        // If not in the dictionary return null
        if (!_prefabs.ContainsKey(name)) return null;
        // Otherwise return the prefab
        return _prefabs[name].NewInstance();
    }

    public WorldTile CreateReserved(WorldLayer layer)
    {
        return new WorldTile(type: TileType.Reserved, buildMode: BuildMode.Single, layer: layer,
            width: 1, height: 1, buildTime: 0, name: "Reserved", movementCost: 1, tile: null,
            buildingAllowed: true, rotations: 0, cost: null, yield: null,
            reserved: true, canRotate: false, requiresUpdate: false);
    }
}