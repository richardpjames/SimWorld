using UnityEngine;

public class JobManager : MonoBehaviour
{
    // The queue of jobs
    public JobQueue JobQueue { get; private set; }

    public static JobManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Create a new queue
        JobQueue = new JobQueue();
    }
}
