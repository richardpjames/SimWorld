using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class WorldTile
{
    public WorldLayer Layer { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int BuildCost { get; protected set; }
    public string Name { get; protected set; }
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
    public bool BuildingAllowed { get; protected set; }
    public bool Walkable { get => MovementCost == 0; }

    public virtual bool CheckValidity(World world, Vector2Int position)
    {
        // Check if we are building on allowable terrain
        if (!world.IsBuildable(position)) return false;
        // Check if another tile is present on this layer
        WorldTile worldTile = world.GetWorldTile(position, Layer);
        if (worldTile != null) return false;
        // Otherwise return true
        return true;
    }
}
