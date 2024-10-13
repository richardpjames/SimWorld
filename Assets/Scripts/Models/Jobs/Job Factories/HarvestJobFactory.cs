using UnityEngine;

public static class HarvestJobFactory
{
    public static Job Create(Vector2Int startPosition, WorldTile startTile, Vector2Int endPosition, WorldTile harvestTile)
    {
        // Get objects from Unity
        World world = GameObject.FindAnyObjectByType<World>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        if (world == null || startPosition == null || startTile == null || endPosition == null || harvestTile == null) return null;
        Job job = new Job();
        job.Cost = null;
        // First we visit the table
        JobStep visitTable = new JobStep(JobType.Harvest, startTile, startPosition, 2, false, 0);
        visitTable.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // Then find the item to be harvested
        WorldTile tile = world.GetWorldTile(endPosition, WorldLayer.Structure);
        if (tile == null) return null;
        // Create the job to harvest (demolish) the item
        JobStep harvest = new JobStep(JobType.Demolish, tile, endPosition, tile.BuildTime, false, tile.Rotations);
        // When complete, this is the work to be done
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