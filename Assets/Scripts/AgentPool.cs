using System;
using System.Collections.Generic;
using System.Linq;
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
            // Remove all agents who have lost their bed
            HashSet<Guid> keys = new HashSet<Guid>();
            foreach (GameObject agent in _agents.Values)
            {
                WorldTile bedTile = _world.GetWorldTile(agent.GetComponent<Agent>().BedLocation, WorldLayer.Structure);
                if (bedTile == null || bedTile.Type != TileType.Bed)
                {
                    // Ensure we have at least one agent (even without a bed)
                    if (_agents.Count > 1)
                    {
                        keys.Add(agent.GetComponent<Agent>().Guid);
                        Destroy(agent);
                    }
                    // If this is our last agent and no bed - set vector 2 out of bunds
                    if (_agents.Count == 1)
                    {
                        agent.GetComponent<Agent>().BedLocation = new Vector2Int(-1, -1);
                    }
                }
            }
            // Remove each of the items from the list
            foreach (Guid agentGuid in keys)
            {
                _agents.Remove(agentGuid);
            }
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
            // Allocate all of the beds to remaining agents
            foreach (GameObject agent in _agents.Values)
            {
                // If the agent doesn't have a bed, then assign one and spawn there
                if (agent.GetComponent<Agent>().BedLocation == Vector2.zero || agent.GetComponent<Agent>().BedLocation == new Vector2Int(-1, -1))
                {
                    // Find a bed and assign to the agent (there may not be one if this is the last agent and no beds)
                    WorldTile tile = _world.Beds.FirstOrDefault<WorldTile>((tile) => tile.Owner == Guid.Empty);
                    if (tile != null)
                    {
                        agent.GetComponent<Agent>().BedLocation = tile.BasePosition;
                        tile.Owner = agent.GetComponent<Agent>().Guid;
                        agent.transform.position = new Vector3(tile.BasePosition.x, tile.BasePosition.y, 0);
                    }
                }
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
            agent.GetComponent<Agent>().BedLocation = _world.Beds[i].BasePosition;
            _world.Beds[i].Owner = agent.GetComponent<Agent>().Guid;
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