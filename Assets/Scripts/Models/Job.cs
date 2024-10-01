using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Job
{
    public Vector2Int Position { get; private set; }
    public Action<Job> OnJobComplete;
    public float JobCost { get; private set; }
    public bool Complete { get; private set; }
    public TileBase Indicator { get; private set; }
    public JobType Type { get; private set; }

    public Job(Vector2Int position, Action<Job> onJobComplete, float jobCost, TileBase indicator = null, JobType jobType = JobType.Structure)
    {
        Position = position;
        OnJobComplete += onJobComplete;
        JobCost = jobCost;
        Complete = false;
        Type = jobType;
        Indicator = indicator;
    }

    public void Work(float points)
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