using UnityEngine;

public class AgentManager : MonoBehaviour
{
    // The number of agents to be created initially
    [SerializeField] private int _initialAgents;
    [SerializeField] private float _agentSpeed;
    // The pool of agents
    public AgentPool AgentPool { get; private set; }

    // Accessor for easy access
    private JobQueue _jobQueue { get => JobManager.Instance.JobQueue; }
    private World _world { get => WorldManager.Instance.World; }

    public static AgentManager Instance { get; private set; }

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
        // Create a new pool with the initial number of agents
        AgentPool = new AgentPool(_initialAgents, _agentSpeed, _jobQueue, new Vector2((int)_world.Size.x / 2, (int)_world.Size.y / 2));
    }

    private void Update()
    {
        AgentPool.Update(Time.deltaTime);
    }
}
