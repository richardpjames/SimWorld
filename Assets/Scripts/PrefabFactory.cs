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
    [SerializeField] private TileBase _treeTile;
    [SerializeField] private TileBase _rockTile;
    [Header("Floors")]
    [SerializeField] private TileBase _woodenFloorTile;
    [SerializeField] private TileBase _stoneFloorTile;

    private Dictionary<string, WorldTile> _prefabs;

    private void Awake()
    {
        // List of all prefabs
        _prefabs = new Dictionary<string, WorldTile>();
        // WorldTile reference to re-use
        WorldTile tile;

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Grass,
            width: 1, height: 1, buildTime: 0, name: "Grass", movementCost: 1, tile: _grassTile,
            buildingAllowed: true, rotations: 0, cost: null, yield: null,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Sand,
            width: 1, height: 1, buildTime: 0, name: "Sand", movementCost: 0.5f, tile: _sandTile,
            buildingAllowed: true, rotations: 0, cost: null, yield: null,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        tile = new WorldTile(type: TileType.Terrain, buildMode: BuildMode.Single, layer: WorldLayer.Water,
            width: 1, height: 1, buildTime: 0, name: "Water", movementCost: 0, tile: _waterTile,
            buildingAllowed: false, rotations: 0, cost: null, yield: null,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenWallCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 2 } };
        tile = new WorldTile(type: TileType.Wall, buildMode: BuildMode.Line, layer: WorldLayer.Structure,
            width: 1, height: 1, buildTime: 1, name: "Wooden Wall", movementCost: 0, tile: _woodenWallTile,
            buildingAllowed: true, rotations: 0, cost: woodenWallCost, yield: woodenWallCost,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenDoorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 3 } };
        tile = new WorldTile(type: TileType.Door, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: 1, buildTime: 1, name: "Wooden Door", movementCost: 1, tile: _woodenDoorTile,
            buildingAllowed: true, rotations: 0, cost: woodenDoorCost, yield: woodenDoorCost,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenBedCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 10 } };
        tile = new WorldTile(type: TileType.Bed, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: -2, buildTime: 3, name: "Wooden Bed", movementCost: 0, tile: _bedTile,
            buildingAllowed: true, rotations: 0, cost: woodenBedCost, yield: woodenBedCost,
            reserved: false, canRotate: true, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> treeYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 2 } };
        tile = new WorldTile(type: TileType.Tree, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: 2, buildTime: 1, name: "Tree", movementCost: 0, tile: _treeTile,
            buildingAllowed: false, rotations: 0, cost: null, yield: treeYield,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> rockYield = new Dictionary<InventoryItem, int>() { { InventoryItem.Stone, 2 } };
        tile = new WorldTile(type: TileType.Rock, buildMode: BuildMode.Single, layer: WorldLayer.Structure,
            width: 1, height: 1, buildTime: 1, name: "Rock", movementCost: 0, tile: _rockTile,
            buildingAllowed: false, rotations: 0, cost: null, yield: rockYield,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> woodenFloorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Wood, 2 } };
        tile = new WorldTile(type: TileType.Floor, buildMode: BuildMode.Drag, layer: WorldLayer.Floor,
            width: 1, height: 1, buildTime: 1, name: "Wooden Floor", movementCost:1, tile: _woodenFloorTile,
            buildingAllowed: true, rotations: 0, cost: woodenFloorCost, yield: woodenFloorCost,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);

        Dictionary<InventoryItem, int> stoneFloorCost = new Dictionary<InventoryItem, int>() { { InventoryItem.Stone, 2 } };
        tile = new WorldTile(type: TileType.Floor, buildMode: BuildMode.Drag, layer: WorldLayer.Floor,
            width: 1, height: 1, buildTime: 1, name: "Stone Floor", movementCost: 1, tile: _stoneFloorTile,
            buildingAllowed: true, rotations: 0, cost: stoneFloorCost, yield: stoneFloorCost,
            reserved: false, canRotate: false, requiresUpdate: false);
        _prefabs.Add(tile.Name, tile);
    }

    public WorldTile GetByName(string name)
    {
        // If not in the dictionary return null
        if (!_prefabs.ContainsKey(name)) return null;
        // Otherwise return the prefab
        return _prefabs[name];
    }

    public WorldTile GetReserved(WorldLayer layer)
    {
        return new WorldTile(type: TileType.Reserved, buildMode: BuildMode.Single, layer: layer,
            width: 1, height: 1, buildTime: 0, name: "Reserved", movementCost: 1, tile: null,
            buildingAllowed: true, rotations: 0, cost: null, yield: null,
            reserved: true, canRotate: false, requiresUpdate: false);
    }
}