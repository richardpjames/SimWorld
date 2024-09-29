using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Job
{
    public Vector2Int Location { get; private set; }
    public Action<Vector2Int> OnJobComplete;
    public float JobCost { get; private set; }
    public bool Complete { get; private set; }
    public TileBase Indicator { get; private set; }

    public Job(Vector2Int location, Action<Vector2Int> onJobComplete, float jobCost, JobTarget jobTarget = JobTarget.Structure)
    {
        Location = location;
        OnJobComplete += onJobComplete;
        JobCost = jobCost;
        Complete = false;
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
                OnJobComplete?.Invoke(Location);
            }
        }
    }
}