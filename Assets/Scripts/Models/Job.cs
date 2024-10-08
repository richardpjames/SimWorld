using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class Job
{
    public Queue<JobStep> JobSteps { get; protected set; }
    public bool Complete { get; protected set; }
    public JobStep CurrentJobStep { get; protected set; }
    public Vector2Int Position { get => CurrentJobStep.Position; }
    public WorldTile WorldTile { get => CurrentJobStep.WorldTile; }
    public JobType Type { get => CurrentJobStep.Type; }
    public TileBase Indicator { get => CurrentJobStep.Indicator; }
    public Quaternion Rotation { get => CurrentJobStep.Rotation; }

    public Action<Job> OnJobComplete;
    public Action<JobStep> OnJobStepComplete;

    public Job(bool complete = false)
    {
        // Initialize the queue and variables
        JobSteps = new Queue<JobStep>();
        Complete = complete;
        CurrentJobStep = null;
    }

    public void AddStep(JobStep step)
    {
        if (CurrentJobStep == null)
        { 
            CurrentJobStep = step; 
        }
        else
        {
            JobSteps.Enqueue(step);
        }
    }

    public static Job DemolishJob(World world, Vector2Int position, WorldLayer layer)
    {
        WorldTile tile = world.GetWorldTile(position, layer);
        if (tile == null) return null;
        Job job = new Job();
        // Create a new job step
        JobStep step = new JobStep(JobType.Demolish, world, tile, position, tile.BuildTime, false, tile.Tile, tile.Rotation);
        // When complete, this is the work to be done
        step.OnJobStepComplete += (job) => { world.RemoveWorldTile(position, tile.Layer); };
        step.OnJobStepComplete += job.OnJobStepComplete;
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

    public static Job BuildJob(World world, Vector2Int position, WorldTile tile, PrefabFactory prefab)
    {
        Job job = new Job();
        // Create a new job step
        JobStep step = new JobStep(JobType.Build, world, tile, position, tile.BuildTime, false, tile.Tile, tile.Rotation);
        step.OnJobStepComplete += (job) => { world.UpdateWorldTile(position, tile); };
        step.OnJobStepComplete += job.OnJobStepComplete;
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
                world.UpdateWorldTile(new Vector2Int(x, y), prefab.GetReserved(tile.Layer));
            }
        }
        // Set as current job step
        job.AddStep(step);
        // Return the job
        return job;
    }

    public static Job HarvestJob(World world, Vector2Int startPosition, WorldTile startTile, Vector2Int endPosition, WorldTile harvestTile)
    {
        Job job = new Job();
        // First we visit the table
        JobStep visitTable = new JobStep(JobType.Harvest,world,startTile,startPosition,2,false,null,Quaternion.identity);
        visitTable.OnJobStepComplete += job.OnJobStepComplete;
        // Then find the item to be harvested
        WorldTile tile = world.GetWorldTile(endPosition, WorldLayer.Structure);
        if (tile == null) return null;
        // Create the job to harvest (demolish) the item
        JobStep harvest = new JobStep(JobType.Demolish, world, tile, endPosition, tile.BuildTime, false, tile.Tile, tile.Rotation);
        // When complete, this is the work to be done
        harvest.OnJobStepComplete += (job) => { world.RemoveWorldTile(endPosition, tile.Layer); };
        harvest.OnJobStepComplete += job.OnJobStepComplete;
        // Reserve tiles for the job
        if (world.GetWorldTile(endPosition, tile.Layer) != null)
        {
            world.GetWorldTile(endPosition, tile.Layer).Reserved = true;
        }
        job.AddStep(visitTable);
        job.AddStep(harvest);
        return job;
    }

    public virtual void Work(float points)
    {
        if (Complete) return;
        if (CurrentJobStep == null || CurrentJobStep.Complete)
        {
            if (JobSteps.Count == 0)
            {
                Complete = true;
                OnJobComplete?.Invoke(this);
                return;
            }
            else
            {
                CurrentJobStep = JobSteps.Dequeue();
            }
        }
        CurrentJobStep.Work(points);
    }
}