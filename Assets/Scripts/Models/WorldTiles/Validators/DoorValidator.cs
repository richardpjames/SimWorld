using UnityEngine;

public static class DoorValidator
{
    public static bool Validate(WorldTile tile, World world, Vector2Int position)
    {
        if (tile.Type == TileType.Door)
        {
            // Must be walls above and below or side to side, but no more than that
            WorldTile wallAbove = world.GetWorldTile(position + Vector2Int.up, WorldLayer.Structure);
            WorldTile wallBelow = world.GetWorldTile(position + Vector2Int.down, WorldLayer.Structure);
            WorldTile wallLeft = world.GetWorldTile(position + Vector2Int.left, WorldLayer.Structure);
            WorldTile wallRight = world.GetWorldTile(position + Vector2Int.right, WorldLayer.Structure);
            // Determine whether there is a wall above, below, left and right
            bool isWallAbove = wallAbove != null && wallAbove.Type == TileType.Wall;
            bool isWallBelow = wallBelow != null && wallBelow.Type == TileType.Wall;
            bool isWallLeft = wallLeft != null && wallLeft.Type == TileType.Wall;
            bool isWallRight = wallRight != null && wallRight.Type == TileType.Wall;
            // If no valid combination (above and below) or (left and right)
            if (!((isWallAbove && isWallBelow) || (isWallLeft && isWallRight))) return false;
            // If above and below, but there are also left or right
            if ((isWallAbove && isWallBelow) && (isWallLeft || isWallRight)) return false;
            // If left and right, but there are also above or below
            if ((isWallLeft && isWallRight) && (isWallAbove || isWallBelow)) return false;
            // Having checked the walls, rotate accordingly
            if (isWallAbove)
            {
                tile.Rotations = 0;
            }
            if (isWallLeft)
            {
                tile.Rotations = 1;
            }
        }
        // If nothing triggered validation (or this is not a door)
        return true;
    }
}