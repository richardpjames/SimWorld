using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Job
{
    public Vector2Int Position { get; protected set; }
    public float JobCost { get; protected set; }
    public bool Complete { get; protected set; }
    public TileBase Indicator { get; protected set; }
    public Quaternion Rotation { get; protected set; }
    public WorldLayer Layer { get; protected set; }

    public Action<Job> OnJobComplete;

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