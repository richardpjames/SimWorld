using UnityEngine;

public static class BuildJobFactory
{
    public static Job Create(Vector2Int position, WorldTile tile)
    {
        // Get objects from Unity
        World world = GameObject.FindAnyObjectByType<World>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        PrefabFactory prefab = GameObject.FindAnyObjectByType<PrefabFactory>();
        // Check we have everything we need
        if (world == null || position == null || tile == null || prefab == null) return null;
        Job job = new Job();
        job.Cost = tile.Cost;
        // Create a new job step
        JobStep step = new JobStep(JobType.Build, tile, position, tile.BuildTime, false, tile.Rotation);
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