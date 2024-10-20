using UnityEngine;

public static class TileValidator
{
    public static bool Validate(WorldTile tile, Vector2Int position)
    {
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + tile.MinX);
        int maxX = Mathf.Max(position.x, position.x + tile.MaxX);
        int minY = Mathf.Min(position.y, position.y + tile.MinY);
        int maxY = Mathf.Max(position.y, position.y + tile.MaxY);
        // Checks for all tiles within the object
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);
                // Check if we are building on allowable terrain
                if (!tile.World.IsBuildable(checkPosition)) return false;
                WorldTile worldTile = tile.World.GetWorldTile(checkPosition, tile.Layer);
                if (worldTile != null) return false;
                // If we are out of bounds then don't build
                if (!tile.World.CheckBounds(checkPosition)) return false;
            }
        }
        // If nothing is invalid then return true
        return true;
    }
}