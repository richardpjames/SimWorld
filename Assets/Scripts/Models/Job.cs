using Codice.Client.Common;
using Codice.CM.Client.Differences;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Job
{
    public JobType Type { get; protected set; }
    public WorldTile WorldTile { get; protected set; }
    public Vector2Int Position { get; protected set; }
    public float JobCost { get; protected set; }
    public bool Complete { get; protected set; }
    public TileBase Indicator { get; protected set; }
    public Quaternion Rotation { get; protected set; }

    public Action<Job> OnJobComplete;

    public Job(JobType type, World world, WorldTile worldTile, Vector2Int position, float jobCost, bool complete, TileBase indicator, Quaternion rotation, PrefabFactory prefab)
    {
        this.Type = type;
        this.WorldTile = worldTile;
        this.Position = position;
        this.JobCost = jobCost;
        this.Complete = complete;
        this.Indicator = indicator;
        this.Rotation = rotation;

        // For building
        if (Type == JobType.Build)
        {
            InitializeBuild(world, position, worldTile, prefab);
        }

        // For demolition
        if (Type == JobType.Demolish)
        {
            InitializeDemolition(world, position);
        }
    }

    private void InitializeBuild(World world, Vector2Int position, WorldTile worldTile, PrefabFactory prefab)
    {
        this.OnJobComplete += (job) => { world.UpdateWorldTile(position, worldTile); };
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
    }

    private void InitializeDemolition(World world, Vector2Int position)
    {
        this.OnJobComplete += (job) => { world.RemoveWorldTile(position, WorldTile.Layer); };
        // Reserve tiles for the job
        if (world.GetWorldTile(position, WorldTile.Layer) != null)
        {
            world.GetWorldTile(position, WorldTile.Layer).Reserved = true;
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
        // If the job is not already complete
        if (JobCost > 0)
        {
            // Subtract the points provided
            JobCost -= points;
            // Check again and invoke if complete
            if (JobCost < 0)
            {
                Complete = true;
                OnJobComplete?.Invoke(this);
            }
        }
    }
}