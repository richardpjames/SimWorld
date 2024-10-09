using UnityEngine;

public static class HarvestJobFactory
{
    public static Job Create(World world, Vector2Int startPosition, WorldTile startTile, Vector2Int endPosition, WorldTile harvestTile)
    {
        if (world == null || startPosition == null || startTile == null || endPosition == null || harvestTile == null) return null;
        Job job = new Job(endPosition, JobType.Demolish);
        job.Cost = null;
        // First we visit the table
        JobStep visitTable = new JobStep(JobType.Harvest, world, startTile, startPosition, 2, false, null, Quaternion.identity);
        visitTable.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // Then find the item to be harvested
        WorldTile tile = world.GetWorldTile(endPosition, WorldLayer.Structure);
        if (tile == null) return null;
        // Create the job to harvest (demolish) the item
        JobStep harvest = new JobStep(JobType.Demolish, world, tile, endPosition, tile.BuildTime, false, tile.Tile, tile.Rotation);
        // When complete, this is the work to be done
        harvest.OnJobStepComplete += (jobStep) => { world.RemoveWorldTile(endPosition, tile.Layer); };
        harvest.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // Reserve tiles for the job
        if (world.GetWorldTile(endPosition, tile.Layer) != null)
        {
            world.GetWorldTile(endPosition, tile.Layer).Reserved = true;
        }
        job.AddStep(harvest);
        job.AddStep(visitTable);
        return job;
    }
}