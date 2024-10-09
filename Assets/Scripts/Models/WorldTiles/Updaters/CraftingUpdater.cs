using UnityEngine;

public static class CraftingUpdater
{
    public static void Update(WorldTile tile, World world, Inventory inventory, JobQueue jobQueue, float delta)
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
                    Vector2Int resourcePosition = world.GetNearestStructure(tile.BasePosition, tile.HarvestType);
                    if (resourcePosition == null) return;
                    // Otherwise get the tile
                    WorldTile resourceTile = world.GetWorldTile(resourcePosition, WorldLayer.Structure);
                    if (resourceTile == null) return;
                    tile.CurrentJob = HarvestJobFactory.Create(world, tile.BasePosition, tile, resourcePosition, resourceTile, inventory);
                }
                else if (tile.Type == TileType.CraftersTable)
                {
                    tile.CurrentJob = CraftJobFactory.Create(world, tile.BasePosition, tile, inventory);
                }
                // If we have been able to create a new job
                if (tile.CurrentJob != null)
                {
                    if (!tile.Continuous) tile.JobCount--;
                    jobQueue.Add(tile.CurrentJob);
                    tile.OnWorldTileUpdated?.Invoke(tile);
                }
            }
        }
    }
}