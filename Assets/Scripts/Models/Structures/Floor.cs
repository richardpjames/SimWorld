using UnityEngine;
using UnityEngine.Tilemaps;

public class Floor : WorldTile
{
    public Floor(string name, float movementCost, TileBase tile, int width, int height, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Floor;
        this.BuildingAllowed = true;
    }
    public override bool CheckValidity(World world, Vector2Int position)
    {
        // Check if we are building on allowable terrain
        WorldTile terrain = world.GetWorldTile(position, WorldLayer.Terrain);
        if (!terrain.BuildingAllowed) return false;
        // Check if another floor is present
        WorldTile floor = world.GetWorldTile(position, WorldLayer.Floor);
        if (floor != null) return false;
        // Otherwise return true
        return true;
    }
}
