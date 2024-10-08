using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    public Job(JobType type, World world, WorldTile worldTile, Vector2Int position, float jobCost, bool complete, TileBase indicator, Quaternion rotation, PrefabFactory prefab)
    {
        // Initialize the queue and variables
        JobSteps = new Queue<JobStep>();
        Complete = complete;
        CurrentJobStep = null;

        // For building
        if (type == JobType.Build)
        {
            InitializeBuild(type, world, worldTile, position, jobCost, complete, indicator, rotation, prefab);
        }

        // For demolition
        if (type == JobType.Demolish)
        {
            InitializeDemolition(type, world, worldTile, position, jobCost, complete, indicator, rotation, prefab);
        }
    }

    private void InitializeBuild(JobType type, World world, WorldTile worldTile, Vector2Int position, float jobCost, bool complete, TileBase indicator, Quaternion rotation, PrefabFactory prefab)
    {
        // Create a new job step
        JobStep step = new JobStep(type, world, worldTile, position, jobCost, complete, indicator, rotation);
        step.OnJobStepComplete += (job) => { world.UpdateWorldTile(position, worldTile); };
        step.OnJobStepComplete += OnJobStepComplete;
        // If this isn't valid, then immediately complete the job and exit
        if (worldTile.CheckValidity(world, position) == false)
        {
            Complete = true;
            return;
        }
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + worldTile.MinX);
        int maxX = Mathf.Max(position.x, position.x + worldTile.MaxX);
        int minY = Mathf.Min(position.y, position.y + worldTile.MinY);
        int maxY = Mathf.Max(position.y, position.y + worldTile.MaxY);
        // Create a number of reserved tiles
        for (int x = minX; x < maxX; x++)
        {
            // Create a number of reserved tiles
            for (int y = minY; y < maxY; y++)
            {
                // Update the world with a reserved tile
                world.UpdateWorldTile(new Vector2Int(x, y), prefab.GetReserved(worldTile.Layer));
            }
        }
        // Set as current job
        AddStep(step);
    }

    private void InitializeDemolition(JobType type, World world, WorldTile worldTile, Vector2Int position, float jobCost, bool complete, TileBase indicator, Quaternion rotation, PrefabFactory prefab)
    {
        // Create a new job step
        JobStep step = new JobStep(type, world, worldTile, position, jobCost, complete, indicator, rotation);
        // When complete, this is the work to be done
        step.OnJobStepComplete += (job) => { world.RemoveWorldTile(position, worldTile.Layer); };
        step.OnJobStepComplete += OnJobStepComplete;
        // Reserve tiles for the job
        if (world.GetWorldTile(position, worldTile.Layer) != null)
        {
            world.GetWorldTile(position, worldTile.Layer).Reserved = true;
        }
        // Add to the queue
        AddStep(step);
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
        return new Job(JobType.Demolish, world, tile, position, tile.BuildTime, false, tile.Tile, Quaternion.identity, null);
    }

    public static Job BuildJob(World world, Vector2Int position, WorldTile tile, PrefabFactory prefab)
    {
        return new Job(JobType.Build, world, tile, position, tile.BuildTime, false, tile.Tile, tile.Rotation, prefab);
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