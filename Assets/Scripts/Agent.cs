using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float _speed;
    public Guid Guid;
    private JobQueue _jobQueue;
    private Inventory _inventory;
    private World _world;
    private Guid _currentJobGuid;
    private Dictionary<InventoryItem, int> _localInventory;
    private Vector2Int _nextPosition;
    private Vector3 _target;
    private AStar _astar;
    private bool _processing = false;

    private void Awake()
    {
        Guid = Guid.NewGuid();
    }

    private void Start()
    {
        _world = GameObject.FindAnyObjectByType<World>();
        _jobQueue = GameObject.FindAnyObjectByType<JobQueue>();
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _localInventory = new Dictionary<InventoryItem, int>();
        _nextPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
    }

    private async void UpdateAstar(JobStep jobStep)
    {
        if (!_processing)
        {
            _processing = true;
            // Set our target
            _target = new Vector3(jobStep.Position.x, jobStep.Position.y, 0);
            // Blank out a start until calculated
            _astar = null;
            // Inputs for a star
            Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Vector2Int end = new Vector2Int((int)_target.x, (int)_target.y);
            // Run the update of the a star algorithm in the background
            await Task.Run(() =>
            {
                _astar = new AStar(start, end, _world);
                _processing = false;
            });
        }
    }

    // Called through a controller to update each agent
    public void Update()
    {
        // Get the job from the register
        Job currentJob = _jobQueue.GetJob(_currentJobGuid);
        // If it no longer exists or is complete then we need to get the next one
        if (currentJob == null || currentJob.Complete)
        {
            // Get the next job from the list
            Job nextJob = _jobQueue.GetNext(this);
            if (nextJob != null)
            {
                _currentJobGuid = nextJob.Guid;
                currentJob = nextJob;
            }
            // If there is a job then update the A Star pathfinding
            if (currentJob != null)
            {
                UpdateAstar(currentJob.CurrentJobStep);
                currentJob.OnNextJobStep += UpdateAstar;
            }
            return;
        }
        if (currentJob != null)
        {
            // Progress the job if we have reached the location
            if (transform.position == _target && !currentJob.Complete)
            {
                currentJob.Work(Time.deltaTime * GameManager.Instance.TimeMultiplier);
                return;
            }
            // Otherwise move towards the location
            if (transform.position != _target && !currentJob.Complete)
            {
                if (_astar != null && transform.position == new Vector3(_nextPosition.x, _nextPosition.y, 0))
                {
                    _nextPosition = _astar.GetNextPosition();
                }
                Vector3 nextPosition3 = new Vector3(_nextPosition.x, _nextPosition.y, 0);
                transform.position = Vector3.MoveTowards(transform.position, nextPosition3, Time.deltaTime * _speed * GameManager.Instance.TimeMultiplier);
                return;
            }
        }
    }

    public AgentSave Serialize()
    {
        // Create a new save and store the location of the agents
        AgentSave save = new AgentSave();
        save.Guid = Guid;
        save.PositionY = transform.position.y;
        save.PositionX = transform.position.x;
        save.CurrentJobGuid = _currentJobGuid;
        return save;
    }

    public void Deserialize(AgentSave save)
    {
        // Initialise the agent as this will be run after awake
        Start();
        // Set the agents ID
        Guid = save.Guid;
        // Get the current job
        _currentJobGuid = save.CurrentJobGuid;
        // Set the position of the agent as per the save
        transform.position = new Vector3(save.PositionX, save.PositionY, transform.position.z);
        Job currentJob = _jobQueue.GetJob(_currentJobGuid);
        if(currentJob != null)
        {
            UpdateAstar(currentJob.CurrentJobStep);
        }
    }
}