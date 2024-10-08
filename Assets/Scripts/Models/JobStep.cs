using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobStep
{
    public JobType Type { get; protected set; }
    public World World { get; protected set; }
    public WorldTile WorldTile { get; protected set; }
    public Vector2Int Position { get; protected set; }
    public float Cost { get; protected set; }
    public bool Complete { get; protected set; }
    public TileBase Indicator { get; protected set; }
    public Quaternion Rotation { get; protected set; }
    public Action<JobStep> OnJobStepComplete;

    public JobStep(JobType type, World world, WorldTile worldTile, Vector2Int position, float cost, bool complete, TileBase indicator, Quaternion rotation)
    {
        World = world;
        Type = type;
        WorldTile = worldTile;
        Position = position;
        Cost = cost;
        Complete = complete;
        Indicator = indicator;
        Rotation = rotation;
    }

    public virtual void Work(float points)
    {
        // If the job is not already complete
        if (Cost > 0)
        {
            // Subtract the points provided
            Cost -= points;
            // Check again and invoke if complete
            if (Cost < 0)
            {
                Complete = true;
                OnJobStepComplete?.Invoke(this);
            }
        }
    }
}