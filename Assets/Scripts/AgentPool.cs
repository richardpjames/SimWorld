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
    private Dictionary<Guid, GameObject> _agents;

    private void Update()
    {
        // Count down the timer
        _currentTimer -= (Time.deltaTime * GameManager.Instance.TimeMultiplier);
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
                    _agents.Add(agent.GetComponent<Agent>().Guid, agent);
                }
            }
            // If we have more villagers than beds
            if (_agents.Count > _world.Beds.Count)
            {
                // Keep a list of Guids to remove
                List<Guid> guidList = new List<Guid>();
                // Build a list of agents to remove
                int i = 0;
                foreach (Guid agentGuid in _agents.Keys)
                {
                    // If we have looped through more than we require (but always ensure 1 agent)
                    if (i >= _world.Beds.Count && i > 0)
                    {
                        guidList.Add(agentGuid);
                    }
                    i++;
                }
                // Now remove any not needed
                foreach (Guid agentGuid in guidList)
                {
                    Destroy(_agents[agentGuid]);
                    _agents.Remove(agentGuid);
                }
            }
            // Allocate all of the beds to remaining agents
            int j = 0;
            foreach (GameObject agent in _agents.Values)
            {
                agent.GetComponent<Agent>().BedLocation = _world.Beds[j].BasePosition;
                j++;
            }
        }
    }

    private void Start()
    {
        _world = GameObject.FindAnyObjectByType<World>();
        // Keep a list of all agents
        _agents = new Dictionary<Guid, GameObject>();
        // Set transform to the center of the world to generate agents
        transform.position = new Vector3(_world.Size.x / 2, _world.Size.y / 2, 0);
        // Generate a number of starting for each bed in the world and keep a list
        for (int i = 0; i < _world.Beds.Count; i++)
        {
            GameObject agent = Instantiate(_agentPrefab, new Vector3(_world.Beds[i].BasePosition.x, _world.Beds[i].BasePosition.y, 0), Quaternion.identity);
            agent.transform.SetParent(transform, true);
            _agents.Add(agent.GetComponent<Agent>().Guid, agent);
        }
        // Start the timer for evaluations
        _currentTimer = _timer;
    }

    public AgentPoolSave Serialize()
    {
        // Create the save
        AgentPoolSave save = new AgentPoolSave();
        // Add each of the agents to a list
        List<AgentSave> list = new List<AgentSave>();
        foreach (GameObject agent in _agents.Values)
        {
            list.Add(agent.GetComponent<Agent>().Serialize());
        }
        //
        save.Agents = list.ToArray();
        // Set the array
        return save;
    }

    public void Deserialize(AgentPoolSave save)
    {
        // Destroy existing agents
        foreach (Guid agentGuid in _agents.Keys)
        {
            Destroy(_agents[agentGuid]);
        }
        // Clear the list
        _agents = new Dictionary<Guid, GameObject>();
        // Create the loaded agents and deserialize them
        foreach (AgentSave agentSave in save.Agents)
        {
            GameObject agent = Instantiate(_agentPrefab, Vector3.zero, Quaternion.identity);
            agent.transform.SetParent(transform, true);
            agent.GetComponent<Agent>().Deserialize(agentSave);
            _agents.Add(agent.GetComponent<Agent>().Guid, agent);
        }
    }
}