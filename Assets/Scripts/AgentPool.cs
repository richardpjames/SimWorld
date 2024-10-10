using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentPool : MonoBehaviour
{
    [SerializeField] GameObject _agentPrefab;
    private World _world;
    private List<GameObject> _agents;


    private void Start()
    {
        _world = GameObject.FindAnyObjectByType<World>();
        // Keep a list of all agents
        _agents = new List<GameObject>();
        // Set transform to the center of the world to generate agents
        transform.position = new Vector3(_world.Size.x / 2, _world.Size.y / 2, 0);
        // Generate a number of starting agents and keep a list
        for (int i = 0; i < GameManager.Instance.StartingAgents; i++)
        {
            GameObject agent = Instantiate(_agentPrefab, transform.position, Quaternion.identity);
            agent.transform.SetParent(transform, true);
            _agents.Add(agent);
        }
    }
}