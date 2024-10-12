using UnityEngine;

public static class CraftJobFactory
{
    public static Job Create(Vector2Int position, WorldTile tile)
    {
        // Get objects from Unity
        World world = GameObject.FindAnyObjectByType<World>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        if (world == null || position == null || tile == null || inventory == null) return null;
        // Create the job and set the cost to the craft cost
        Job job = new Job();
        job.Cost = tile.CraftCost;
        // Create the actual step to craft the item and hook up the triggers
        JobStep step = new JobStep(JobType.Craft, world, tile, inventory, position, tile.CraftTime, false, null, Quaternion.identity);
        step.OnJobStepComplete += (jobStep) => { inventory.Add(tile.CraftYield); };
        step.OnJobStepComplete += job.TriggerOnJobStepComplete;
        // Add the step to the job and return
        job.AddStep(step);
        return job;
    }
}