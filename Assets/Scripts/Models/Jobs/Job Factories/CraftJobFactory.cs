using UnityEngine;

public static class CraftJobFactory
{
    public static Job Create(World world, Vector2Int position, WorldTile tile, Inventory inventory)
    {
        if (world == null || position == null || tile == null || inventory == null) return null;
        // Create the job and set the cost to the craft cost
        Job job = new Job(position, JobType.Craft);
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