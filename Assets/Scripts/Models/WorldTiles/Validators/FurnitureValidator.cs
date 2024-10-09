using UnityEngine;

public static class FurnitureValidator
{
    public static bool Validate(WorldTile tile, World world, Vector2Int position)
    {
        // Check for types of furniture
        if (tile.Type == TileType.Bed || tile.Type == TileType.Chair || tile.Type == TileType.Table)
        {
            // This must be placed inside
            if (!world.IsInside(position)) return false;
        }
        // If no issues, or not furniture return true
        return true;
    }
}