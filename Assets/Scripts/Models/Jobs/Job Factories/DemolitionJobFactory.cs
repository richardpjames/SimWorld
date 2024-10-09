using UnityEngine;

public static class DemolitionJobFactory
{
    public static Job Create(World world, Vector2Int position, WorldLayer layer, Inventory inventory)
    {
        WorldTile tile = world.GetWorldTile(position, layer);
        if (tile == null) return null;
        Job job = new Job(position, JobType.Demolish);
        job.Cost = null;
        // Create a new job step
        JobStep step = new JobStep(JobType.Demolish, world, tile, inventory, position, tile.BuildTime, false, tile.Tile, tile.Rotation);
        // When complete, this is the work to be done
        step.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // Reserve tiles for the job
        if (world.GetWorldTile(position, tile.Layer) != null)
        {
            world.GetWorldTile(position, tile.Layer).Reserved = true;
        }
        // Add to the queue
        job.AddStep(step);
        // Return the job
        return job;
    }
}