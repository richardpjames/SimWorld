using UnityEngine;

public static class BuildJobFactory
{
    public static Job Create(World world, Vector2Int position, WorldTile tile, PrefabFactory prefab)
    {
        if (world == null || position == null || tile == null || prefab == null) return null;
        Job job = new Job(position, JobType.Build);
        job.Cost = tile.Cost;
        // Create a new job step
        JobStep step = new JobStep(JobType.Build, world, tile, position, tile.BuildTime, false, tile.Tile, tile.Rotation);
        step.OnJobStepComplete += (jobStep) => { world.UpdateWorldTile(position, tile); };
        step.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // If this isn't valid, then immediately complete the job and exit
        if (tile.CheckValidity(world, position) == false)
        {
            job.Complete = true;
            return job;
        }
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + tile.MinX);
        int maxX = Mathf.Max(position.x, position.x + tile.MaxX);
        int minY = Mathf.Min(position.y, position.y + tile.MinY);
        int maxY = Mathf.Max(position.y, position.y + tile.MaxY);
        // Create a number of reserved tiles
        for (int x = minX; x < maxX; x++)
        {
            // Create a number of reserved tiles
            for (int y = minY; y < maxY; y++)
            {
                // Update the world with a reserved tile
                world.UpdateWorldTile(new Vector2Int(x, y), prefab.CreateReserved(tile.Layer));
            }
        }
        // Set as current job step
        job.AddStep(step);
        // Return the job
        return job;
    }
}