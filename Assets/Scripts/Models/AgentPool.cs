using System;
using System.Collections.Generic;

public class AgentPool
{
    public List<Agent> Agents { get; private set; }
    public Action<Agent> OnAgentCreated;
    public Action<Agent> OnAgentUpdated;

    public AgentPool(int size, float speed, JobQueue jobQueue)
    {
        // Create a pool of the provided size
        Agents = new List<Agent>();
        // Loop through and create
        for (int i = 0; i < size; i++)
        {
            // Create the agent and add to the list
            Agent agent = new Agent(speed, jobQueue);
            Agents.Add(agent);
            // Invoke the message to let others know the agent is created
            OnAgentCreated?.Invoke(agent);
        }
    }

    public void Update(float deltaTime)
    {
        // Loop through each agent in the pool
        foreach (Agent agent in Agents)
        {
            // Update and tell others that the update is complete
            agent.Update(deltaTime);
            OnAgentUpdated?.Invoke(agent);
        }
    }
}