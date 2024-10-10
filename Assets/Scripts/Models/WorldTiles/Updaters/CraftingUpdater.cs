using UnityEngine;

public static class CraftingUpdater
{
    public static void Update(WorldTile tile, float delta)
    {
        // For harvesters like the woodcutters table
        if (tile.Type == TileType.HarvestersTable || tile.Type == TileType.CraftersTable)
        {
            // If no jobs have been created, or the current job is complete
            if ((tile.CurrentJob == null || tile.CurrentJob.Complete) && (tile.JobCount > 0 || tile.Continuous))
            {
                // Create a new job to harvest the resource
                if (tile.Type == TileType.HarvestersTable)
                {
                    // Get the nearest structure we want to harvest but return if there are none
                    Vector2Int resourcePosition = tile.World.GetNearestStructure(tile.BasePosition, tile.HarvestType);
                    if (resourcePosition == null) return;
                    // Otherwise get the tile
                    WorldTile resourceTile = tile.World.GetWorldTile(resourcePosition, WorldLayer.Structure);
                    if (resourceTile == null) return;
                    tile.CurrentJob = HarvestJobFactory.Create(tile.WorkPosition, tile, resourcePosition, resourceTile);
                }
                else if (tile.Type == TileType.CraftersTable)
                {
                    tile.CurrentJob = CraftJobFactory.Create(tile.BasePosition, tile);
                }
                // If we have been able to create a new job
                if (tile.CurrentJob != null)
                {
                    if (!tile.Continuous) tile.JobCount--;
                    tile.JobQueue.Add(tile.CurrentJob);
                    tile.OnWorldTileUpdated?.Invoke(tile);
                }
            }
        }
    }
}