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
    public Vector2Int BedLocation;
    public float Energy;
    public bool NeedsSleep;

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
        Energy = 100;
        NeedsSleep = false;
    }

    private void UpdateAstarJobStep(JobStep jobStep)
    {
        // Simpply call the update with the position from the next job step
        UpdateAstar(jobStep.Position);
    }

    private async void UpdateAstar(Vector2Int position)
    {
        if (!_processing)
        {
            try
            {
                _processing = true;
                // Set our target
                _target = new Vector3(position.x, position.y, 0);
                // Blank out a start until calculated
                _astar = null;
                // Inputs for a star
                Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
                Vector2Int end = new Vector2Int((int)_target.x, (int)_target.y);
                // Run the update of the a star algorithm in the background
                await Task.Run(() =>
                {
                    AStar newAstar = new AStar(start, end, _world);
                    _astar = newAstar;
                    _processing = false;
                });
            }
            // In the case of exception
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    // Called through a controller to update each agent
    public void Update()
    {
        // Deal with whether the agent needs sleep
        if (Energy <= 0) NeedsSleep = true;
        if (Energy >= 100) NeedsSleep = false;
        // If we are in bed, then regenerate energy each frame
        if (transform.position == new Vector3(BedLocation.x, BedLocation.y, 0))
        {
            // Energy regerenates at double speed
            Energy += Time.deltaTime * GameManager.Instance.TimeMultiplier * 2;
            // Cap energy at 100
            Energy = Mathf.Min(Energy, 100);
        }
        // Get the job from the register
        Job currentJob = _jobQueue.GetJob(_currentJobGuid);
        // If it no longer exists or is complete (and we don't need sleep) then we need to get the next one
        if ((currentJob == null || currentJob.Complete) && !NeedsSleep)
        {
            // Get the next job from the list
            Job nextJob = _jobQueue.GetNext(this);
            if (nextJob != null)
            {
                _currentJobGuid = nextJob.Guid;
                currentJob = nextJob;
            }
            // If there is a job then update the A Star pathfinding to the job
            if (currentJob != null)
            {
                UpdateAstar(currentJob.CurrentJobStep.Position);
                currentJob.OnNextJobStep += UpdateAstarJobStep;
            }
            return;
        }
        // Otherwise, if the job is compete but we have no energy and are not heading to bed
        else if ((currentJob == null || currentJob.Complete) && NeedsSleep && _target != new Vector3(BedLocation.x, BedLocation.y, 0))
        {
            currentJob = null;
            UpdateAstar(BedLocation);
            return;
        }
        if (currentJob != null)
        {
            // If we are doing a job, then expend energy
            Energy -= Time.deltaTime * GameManager.Instance.TimeMultiplier;
            // Cap at zero
            Energy = Mathf.Max(Energy, 0);
            // Progress the job if we have reached the location
            if (transform.position == _target && !currentJob.Complete)
            {
                currentJob.Work(Time.deltaTime * GameManager.Instance.TimeMultiplier);
                return;
            }
        }
        // Move towards the target (either a job or bed)
        if (transform.position != _target)
        {
            if (_astar != null && transform.position == new Vector3(_nextPosition.x, _nextPosition.y, 0))
            {
                _nextPosition = _astar.GetNextPosition();
            }
            Vector3 nextPosition3 = new Vector3(_nextPosition.x, _nextPosition.y, 0);
            transform.position = Vector3.MoveTowards(transform.position, nextPosition3, Time.deltaTime * _speed * GameManager.Instance.TimeMultiplier);
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
        save.TargetX = _target.x;
        save.TargetY = _target.y;
        save.TargetZ = _target.z;
        save.NextPositionX = _nextPosition.x;
        save.NextPositionY = _nextPosition.y;
        save.BedLocationX = BedLocation.x;
        save.BedLocationY = BedLocation.y;
        save.Energy = Energy;
        save.NeedsSleep = NeedsSleep;
        if (_astar != null)
        {
            save.AStar = _astar.Serialize();
        }
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
        // Get the location of this agents bed
        BedLocation = new Vector2Int(save.BedLocationX, save.BedLocationY);
        // The characters energy
        Energy = save.Energy;
        NeedsSleep = save.NeedsSleep;
        // Get target and next position
        _target = new Vector3(save.TargetX, save.TargetY, save.TargetZ);
        _nextPosition = new Vector2Int(save.NextPositionX, save.NextPositionY);
        // Get the a star from the save
        if (save.AStar != null)
        {
            _astar = save.AStar.Deserialize();
        }
        Job currentJob = _jobQueue.GetJob(_currentJobGuid);
        if (currentJob != null)
        {
            // Make sure that jobs are hooked up
            currentJob.OnNextJobStep += UpdateAstarJobStep;
        }
    }

    private void OnDestroy()
    {
        // If this agent has a job assigned
        if (_currentJobGuid != Guid.Empty)
        {
            // And we can get that job from the register
            Job job = _jobQueue.GetJob(_currentJobGuid);
            if (job != null)
            {
                // Then make it available for other agents to pick up
                job.AssignedAgent = Guid.Empty;
            }
        }
    }
}