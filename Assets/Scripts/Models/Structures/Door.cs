using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : WorldTile
{
    public Door(string name, float movementCost, TileBase tile, Quaternion rotation, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = 1;
        this.Height = 1;
        this._originalHeight = Height;
        this._originalWidth = Width;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
        this.Rotation = rotation;
        this.BuildMode = BuildMode.Single;
        this._canRotate = false;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Door(Name, MovementCost, Tile, Rotation, BuildCost);
    }

    public override bool CheckValidity(World world, Vector2Int position)
    {
        if(!base.CheckValidity(world, position)) return false;
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
        // Having checked the walls, rotate accordingly
        if(isWallAbove)
        {
            Rotation = Quaternion.identity;
        }
        if(isWallLeft)
        {
            Rotation = Quaternion.Euler(0, 0, -90f);
        }
        // Otherwise return true
        return true;
    }
}
