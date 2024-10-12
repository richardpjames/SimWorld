using System;

[Serializable]
public class JobQueueSave
{
    public Guid[] JobQueue;
    public JobSave[] Register;
}