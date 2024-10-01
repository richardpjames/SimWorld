using System.Collections.Generic;
using UnityEngine;

public class Agent
{
    public Vector2 Position { get; private set; }
    private float speed;
    private JobQueue jobQueue;
    private Job currentJob;
    public Agent(float speed, JobQueue jobQueue)
    {
        this.jobQueue = jobQueue;
        Position = Vector2.zero;
        this.speed = speed;
    }

    // Called through a controller to update each agent
    public void Update(float deltaTime)
    {
        
        if (currentJob == null || currentJob.Complete)
        {
            // Get the next job from the list
            currentJob = jobQueue.GetNext();
            return;
        }
        if (currentJob != null)
        {
            // Progress the job if we have reached the location
            if (Position == currentJob.Position && !currentJob.Complete)
            {
                currentJob.Work(deltaTime);
                return;
            }
            // Otherwise move towards the location
            if (Position != currentJob.Position && !currentJob.Complete)
            {
                Position = Vector2.MoveTowards(Position, currentJob.Position, deltaTime * speed);
                return;
            }
        }
    }
}