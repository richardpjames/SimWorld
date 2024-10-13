using System;
using System.Collections.Generic;

[Serializable]
public class JobSave
{
    public Guid Guid;
    public Guid AssignedAgent;
    public JobStepSave[] JobSteps;
    public bool Complete;
    public InventoryItemSave[] Cost;
    public float TimeQueued;

    public Job Deserialize()
    {
        Job job = new Job();
        job.Guid = Guid;
        job.Complete = Complete;
        job.AssignedAgent = AssignedAgent;
        job.TimeQueued = TimeQueued;
        // Clear the existing cost
        job.Cost = new Dictionary<InventoryItem, int>();
        // Create a linked list of the job steps
        foreach(JobStepSave step in JobSteps)
        {
            // Loop through and add each to the first (to reverse the queue)
            JobStep jobStep = step.Deserialize();
            job.JobSteps.AddFirst(jobStep);
            if(jobStep != null && !jobStep.Complete)
            {
                job.CurrentJobStep = jobStep;
            }
            jobStep.OnJobStepComplete += job.TriggerOnJobStepComplete;
        }
        // Load each of the saved cost elements
        foreach (InventoryItemSave item in Cost)
        {
            job.Cost.Add((InventoryItem)item.Item, item.Amount);
        }
        return job;
    }
}