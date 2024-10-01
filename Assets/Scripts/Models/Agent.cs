using System.Collections.Generic;
using UnityEngine;

public class Agent
{
    public Vector2 Position { get; private set; }
    private float speed;
    private Queue<Job> jobQueue;
    private Job currentJob;
    public Agent(float speed, Queue<Job> jobQueue)
    {
        this.jobQueue = jobQueue;
        Position = Vector2.zero;
        this.speed = speed;
    }

    // Called through a controller to update each agent
    public void Update(float deltaTime)
    {
        
        if (jobQueue.Count > 0 && (currentJob == null || currentJob.Complete))
        {
            // Get the next job from the list
            currentJob = jobQueue.Dequeue();
            return;
        }
        if (currentJob != null)
        {
            Vector2 targetLocation = new Vector2(currentJob.Location.x + 0.5f, currentJob.Location.y + 0.5f);
            // Progress the job if we have reached the location
            if (Position == targetLocation && !currentJob.Complete)
            {
                currentJob.Work(deltaTime);
                return;
            }
            // Otherwise move towards the location
            if (Position != targetLocation && !currentJob.Complete)
            {
                Position = Vector2.MoveTowards(Position, targetLocation, deltaTime * speed);
                return;
            }
        }

    }
}