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
        tile = new Terrain(name: "Grass", tile: _grassTile, rotation: Quaternion.identity, movementCost: 1, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Grass);
        _prefabs.Add(tile.Name, tile);
        tile = new Terrain(name: "Sand", tile: _sandTile, rotation: Quaternion.identity, movementCost: 0.5f, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Sand);
        _prefabs.Add(tile.Name, tile);
        tile = new Terrain(name: "Water", tile: _waterTile, rotation: Quaternion.identity, movementCost: 0, width: 1, height: 1, buildCost: 0, layer: WorldLayer.Water, buildingAllowed: false);
        _prefabs.Add(tile.Name, tile);
        tile = new Wall(name: "Wooden Wall", tile: _woodenWallTile, rotation: Quaternion.identity, movementCost: 1, width: 1, height: 1, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
        tile = new Door(name: "Wooden Door", tile: _woodenDoorTile, rotation: Quaternion.identity, movementCost: 1, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
        tile = new Bed(name: "Wooden Bed", tile: _bedTile, rotation: Quaternion.identity, movementCost: 0, width: 1, height: -2, buildCost: 3);
        _prefabs.Add(tile.Name, tile);
        tile = new Tree(name: "Tree", tile: _treeTile, rotation: Quaternion.identity, movementCost: 0, width: 1, height: 2, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
        tile = new Rock(name: "Rock", tile: _rockTile, rotation: Quaternion.identity, movementCost: 0.5f, width: 1, height: 1, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
        tile = new Floor(name: "Wooden Floor", tile: _woodenFloorTile, rotation: Quaternion.identity, movementCost: 1, width: 1, height: 1, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
        tile = new Floor(name: "Stone Floor", tile: _stoneFloorTile, rotation: Quaternion.identity, movementCost: 1, width: 1, height: 1, buildCost: 1);
        _prefabs.Add(tile.Name, tile);
    }

    public WorldTile GetByName(string name)
    {
        // If not in the dictionary return null
        if (!_prefabs.ContainsKey(name)) return null;
        // Otherwise return the prefab
        return _prefabs[name];
    }
}