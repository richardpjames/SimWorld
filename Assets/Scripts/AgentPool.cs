using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AgentPool : MonoBehaviour
{
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private float _timer;
    private float _currentTimer;
    private World _world;
    private List<GameObject> _agents;

    private void Update()
    {
        // Count down the timer
        _currentTimer -= Time.deltaTime;
        // If the timer has passed
        if (_currentTimer < 0)
        {
            // Reset the timer
            _currentTimer = _timer;
            // If we have more beds than villagers
            if (_agents.Count < _world.Beds.Count)
            {
                int numberToGenerate = _world.Beds.Count - _agents.Count;
                for (int x = 0; x < numberToGenerate; x++)
                {
                    GameObject agent = Instantiate(_agentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    agent.transform.SetParent(transform, true);
                    _agents.Add(agent);
                }
            }
            // If we have more villagers than beds
            if (_agents.Count > _world.Beds.Count)
            {
                // Determine how many agents we need to remove
                int numberToRemove = _agents.Count - _world.Beds.Count;
                // Ensure that we will always have at least one agent left
                numberToRemove = Mathf.Min(numberToRemove, _agents.Count - 1);
                for (int x = 0; x < numberToRemove; x++)
                {
                    Destroy(_agents[0]);
                    _agents.RemoveAt(0);
                }
            }
        }
    }

    private void Start()
    {
        _world = GameObject.FindAnyObjectByType<World>();
        // Keep a list of all agents
        _agents = new List<GameObject>();
        // Set transform to the center of the world to generate agents
        transform.position = new Vector3(_world.Size.x / 2, _world.Size.y / 2, 0);
        // Generate a number of starting for each bed in the world and keep a list
        for (int i = 0; i < _world.Beds.Count; i++)
        {
            GameObject agent = Instantiate(_agentPrefab, new Vector3(_world.Beds[i].BasePosition.x, _world.Beds[i].BasePosition.y, 0), Quaternion.identity);
            agent.transform.SetParent(transform, true);
            _agents.Add(agent);
        }
        // Start the timer for evaluations
        _currentTimer = _timer;
    }
}