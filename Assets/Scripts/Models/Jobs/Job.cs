using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Job
{
    public Guid Guid { get; internal set; }
    public Guid AssignedAgent { get; internal set; }
    public LinkedList<JobStep> JobSteps { get; internal set; }
    public bool Complete { get; set; }
    public JobStep CurrentJobStep { get; internal set; }
    public float TimeQueued { get; internal set; }
    public Dictionary<InventoryItem, int> Cost { get; set; }
    public Action<JobStep> OnJobStepComplete;
    public Action<JobStep> OnNextJobStep;
    public Action<Job> OnJobComplete;

    public Job()
    {
        // Create a guid
        Guid = Guid.NewGuid();
        // Initialize the queue and variables
        JobSteps = new LinkedList<JobStep>();
        Complete = false;
        CurrentJobStep = null;
    }
    public void AddStep(JobStep step)
    {
        // If the current job step is empty then assign here so picked up straight away
        if (CurrentJobStep == null)
        {
            CurrentJobStep = step;
        }
        JobSteps.AddFirst(step);
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
            if (JobSteps.Count((step) => step.Complete == false) == 0)
            {
                Complete = true;
                OnJobComplete?.Invoke(this);
                return;
            }
            // Otherwise pick up the next job step and invoke the action to let others know
            else
            {
                // Get the first item in the list which is not complete
                CurrentJobStep = JobSteps.FirstOrDefault<JobStep>((step) => step.Complete == false);
                OnNextJobStep?.Invoke(CurrentJobStep);
            }
        }
        // Finally, we apply work to any action we are currently performing 
        CurrentJobStep.Work(points);
    }

    public JobSave Serialize()
    {
        JobSave save = new JobSave();
        // Set basic data
        save.Guid = Guid;
        save.Complete = Complete;
        save.AssignedAgent = AssignedAgent;
        save.TimeQueued = TimeQueued;
        // Gather all of the job steps
        List<JobStepSave> jobStepSaves = new List<JobStepSave>();
        foreach (JobStep step in JobSteps)
        {
            jobStepSaves.Add(step.Serialize());
        }
        save.JobSteps = jobStepSaves.ToArray();
        // Loop through each cost item and add it
        List<InventoryItemSave> saveItems = new List<InventoryItemSave>();
        if (Cost != null)
        {
            foreach (InventoryItem item in Cost.Keys)
            {
                // Create an object for each type and populate it
                InventoryItemSave itemSave = new InventoryItemSave();
                // Store the item and amount
                itemSave.Item = (int)item;
                itemSave.Amount = Cost[item];
                // Add it to the temporary list
                saveItems.Add(itemSave);
            }
        }
        save.Cost = saveItems.ToArray();
        return save;
    }
}