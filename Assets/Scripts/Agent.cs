using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float _speed;
    private JobQueue _jobQueue;
    private Inventory _inventory;
    private Job _currentJob;
    private Dictionary<InventoryItem, int> _localInventory;

    private void Start()
    {
        _jobQueue = GameObject.FindAnyObjectByType<JobQueue>();
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _localInventory = new Dictionary<InventoryItem, int>();
    }

    // Called through a controller to update each agent
    public void Update()
    {
        
        if (_currentJob == null || _currentJob.Complete)
        {
            // Get the next job from the list
            _currentJob = _jobQueue.GetNext();
            return;
        }
        if (_currentJob != null)
        {
            Vector3 target = new Vector3(_currentJob.CurrentJobStep.Position.x, _currentJob.CurrentJobStep.Position.y, 0);
            // Progress the job if we have reached the location
            if (transform.position == target && !_currentJob.Complete)
            {
                _currentJob.Work(Time.deltaTime);
                return;
            }
            // Otherwise move towards the location
            if (transform.position != target && !_currentJob.Complete)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * _speed);
                return;
            }
        }
    }
}