using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : WorldTile
{
    public Wall(string name, float movementCost, TileBase tile, int width, int height, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
    }

    public override bool CheckValidity(World world, Vector2Int position)
    {
        // Check if we are building on water
        WorldTile terrain = world.GetWorldTile(position, WorldLayer.Terrain);
        if (!terrain.BuildingAllowed) return false;
        // Check if another structure is present
        WorldTile structure = world.GetWorldTile(position,WorldLayer.Structure);
        if (structure != null) return false;
        // Otherwise return true
        return true;
    }
}
