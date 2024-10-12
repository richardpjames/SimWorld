using UnityEngine;

public static class DemolitionJobFactory
{
    public static Job Create(Vector2Int position, WorldLayer layer)
    {
        // Get objects from Unity
        World world = GameObject.FindAnyObjectByType<World>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        WorldTile tile = world.GetWorldTile(position, layer);
        if (tile == null) return null;
        Job job = new Job();
        job.Cost = null;
        // Create a new job step
        JobStep step = new JobStep(JobType.Demolish, tile, position, tile.BuildTime, false, tile.Rotation);
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