using System;
using System.Collections.Generic;
using System.Linq;

public class JobQueue
{
    // The queue of jobs
    private Queue<Job> _queue;
    // Actions to let others know these are created and completed
    public Action<Job> OnJobCreated;
    public Action<Job> OnJobCompleted;

    private World _world { get => WorldController.Instance.World; }

    public JobQueue()
    {
        _queue = new Queue<Job>();
    }

    // Add a job to the queue
    public bool Add(Job job)
    {
        // Check if any jobs of the same type already exist at this position
        if (_queue.Any<Job>((queuedJob) => queuedJob.Position == job.Position && queuedJob.Type == job.Type))
        {
            return false;
        }
        // Special checks for building
        if (job.Type == JobType.Structure)
        {
            if(_world.Get<Structure>(job.Position) != null || _world.Get<Terrain>(job.Position).TerrainType == TerrainType.Water)
            {
                return false;
            }
        }
        // Special checks for building
        if (job.Type == JobType.Floor)
        {
            if (_world.Get<Floor>(job.Position) != null || _world.Get<Terrain>(job.Position).TerrainType == TerrainType.Water)
            {
                return false;
            }
        }
        // If not then add to the queue
        _queue.Enqueue(job);
        // Hook up the actions
        job.OnJobComplete += OnJobCompleted;
        // Notify that a job is created
        OnJobCreated?.Invoke(job);
        // Return true to notify complete
        return true;
    }

    /// <summary>
    /// Get the next available job from the queue
    /// </summary>
    /// <returns></returns>
    public Job GetNext()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }
        return null;
    }

}