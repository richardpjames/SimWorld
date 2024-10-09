using System;
using System.Collections.Generic;
using UnityEngine;

public class Job
{
    public Queue<JobStep> JobSteps { get; protected set; }
    public bool Complete { get; set; }
    public JobStep CurrentJobStep { get; protected set; }
    public Dictionary<InventoryItem, int> Cost { get; set; }
    public Action<JobStep> OnJobStepComplete;
    public Action<JobStep> OnNextJobStep;

    public Job(Vector2Int position, JobType type, bool complete = false)
    {
        // Initialize the queue and variables
        JobSteps = new Queue<JobStep>();
        Complete = complete;
        CurrentJobStep = null;
    }
    public void AddStep(JobStep step)
    {
        // If the current job step is empty then assign here so picked up straight away
        if (CurrentJobStep == null)
        {
            CurrentJobStep = step;
        }
        // Otherwise add it to the queue to be picked up in order
        else
        {
            JobSteps.Enqueue(step);
        }
    }
    // When job steps are complete we need to let others know, this provides the plumbing to do that
    public void TriggerOnJobStepComplete(JobStep jobStep)
    {
        OnJobStepComplete?.Invoke(jobStep);
    }
    // This is called whenever an agent performs work on a job
    public virtual void Work(float points)
    {
        // If the work is complete then we cannot continue further
        if (Complete) return;
        // If the current step is complete then we need to either...
        if (CurrentJobStep == null || CurrentJobStep.Complete)
        {
            // Mark the job as complete if there are no steps remaining
            if (JobSteps.Count == 0)
            {
                Complete = true;
                return;
            }
            // Otherwise pick up the next job step and invoke the action to let others know
            else
            {
                CurrentJobStep = JobSteps.Dequeue();
                OnNextJobStep?.Invoke(CurrentJobStep);
            }
        }
        // Finally, we apply work to any action we are currently performing 
        CurrentJobStep.Work(points);
    }
}