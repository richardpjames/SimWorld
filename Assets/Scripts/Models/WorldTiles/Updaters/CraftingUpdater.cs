using UnityEngine;

public static class CraftingUpdater
{
    public static void Update(WorldTile tile, float delta)
    {
        // For harvesters like the woodcutters table
        if (tile.Type == TileType.HarvestersTable || tile.Type == TileType.CraftersTable)
        {
            // Get the current job from the job register
            Job job = tile.JobQueue.GetJob(tile.CurrentJob);
            // For holding the next job if it is created
            Job nextJob = null;
            // If no jobs have been created, or the current job is complete
            if ((job == null || job.Complete) && (tile.JobCount > 0 || tile.Continuous))
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
                    nextJob = HarvestTableJobFactory.Create(tile.WorkPosition, tile, resourcePosition, resourceTile);
                }
                else if (tile.Type == TileType.CraftersTable)
                {
                    nextJob = CraftJobFactory.Create(tile.WorkPosition, tile);
                }
                // If we have been able to create a new job
                if (nextJob != null)
                {
                    if (!tile.Continuous) tile.JobCount--;
                    tile.CurrentJob = nextJob.Guid;
                    tile.JobQueue.Add(nextJob);
                    tile.OnWorldTileUpdated?.Invoke(tile);
                }
            }
        }
    }
}