using UnityEngine;

public static class CropValidator
{
    public static bool Validate(WorldTile tile, Vector2Int position)
    {
        // Check for types of furniture
        if (tile.Type == TileType.Crop || tile.Type == TileType.CropField || tile.Type == TileType.Sapling)
        {
            World world = GameObject.FindAnyObjectByType<World>();
            WorldTile floor = world.GetWorldTile(position, WorldLayer.Floor);
            // Cannot be build on top of flooring
            if (floor != null) return false;
            // This must be placed outside
            if (tile.World.IsInside(position)) return false;
        }
        // If no issues, or not furniture return true
        return true;
    }
}