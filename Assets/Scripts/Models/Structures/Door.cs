using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : WorldTile
{
    public Door(string name, float movementCost, TileBase tile, int width, int height, int buildCost)
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
        // Must be walls above and below or side to side, but no more than that
        WorldTile wallAbove = world.GetWorldTile(position + Vector2Int.up, WorldLayer.Structure);
        WorldTile wallBelow = world.GetWorldTile(position + Vector2Int.down, WorldLayer.Structure);
        WorldTile wallLeft = world.GetWorldTile(position + Vector2Int.left, WorldLayer.Structure);
        WorldTile wallRight = world.GetWorldTile(position + Vector2Int.right, WorldLayer.Structure);
        // Determine whether there is a wall above, below, left and right
        bool isWallAbove = wallAbove != null && wallAbove.GetType() == typeof(Wall);
        bool isWallBelow = wallBelow != null && wallBelow.GetType() == typeof(Wall);
        bool isWallLeft = wallLeft != null && wallLeft.GetType() == typeof(Wall);
        bool isWallRight = wallRight != null && wallRight.GetType() == typeof(Wall);
        // If no valid combination (above and below) or (left and right)
        if (!((isWallAbove && isWallBelow) || (isWallLeft && isWallRight))) return false;
        // If above and below, but there are also left or right
        if((isWallAbove && isWallBelow) && (isWallLeft || isWallRight)) return false;
        // If left and right, but there are also above or below
        if ((isWallLeft && isWallRight) && (isWallAbove || isWallBelow)) return false;
        // Check if we are building on allowable terrain
        if (!world.IsBuildable(position)) return false;
        // Check if another structure is present
        WorldTile structure = world.GetWorldTile(position, WorldLayer.Structure);
        if (structure != null) return false;
        // Otherwise return true
        return true;
    }
}
