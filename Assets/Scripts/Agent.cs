using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float _speed;
    private JobQueue _jobQueue;
    private Inventory _inventory;
    private World _world;
    private Job _currentJob;
    private Dictionary<InventoryItem, int> _localInventory;
    private Vector2Int _nextPosition;
    private Vector3 _target;
    private AStar _astar;
    private bool _processing = false;

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

        if (_currentJob == null || _currentJob.Complete)
        {
            // Get the next job from the list
            _currentJob = _jobQueue.GetNext();
            // If there is a job then update the A Star pathfinding
            if (_currentJob != null)
            {
                UpdateAstar(_currentJob.CurrentJobStep);
                _currentJob.OnNextJobStep += UpdateAstar;
            }
            return;
        }
        if (_currentJob != null)
        {
            // Progress the job if we have reached the location
            if (transform.position == _target && !_currentJob.Complete)
            {
                _currentJob.Work(Time.deltaTime);
                return;
            }
            // Otherwise move towards the location
            if (transform.position != _target && !_currentJob.Complete)
            {
                if (_astar != null && transform.position == new Vector3(_nextPosition.x, _nextPosition.y, 0))
                {
                    _nextPosition = _astar.GetNextPosition();
                }
                Vector3 nextPosition3 = new Vector3(_nextPosition.x, _nextPosition.y, 0);
                transform.position = Vector3.MoveTowards(transform.position, nextPosition3, Time.deltaTime * _speed);
                return;
            }
        }
    }
}