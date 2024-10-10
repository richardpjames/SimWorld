using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStar
{
    public Queue<Vector2Int> Path { get; private set; }
    public AStar(Vector2Int startPosition, Vector2Int endPosition, World world)
    {
        // Initialise the path list
        Path = new Queue<Vector2Int>();
        // Get the navigation graph from the world
        NavigationGraph graph = world.NavigationGraph;
        // Check that we are on a start tile which is in the graph and asking for an end tile on the graph
        if (!graph.Nodes.ContainsKey(startPosition) || !graph.Nodes.ContainsKey(endPosition)) return;
        // Those nodes already evaluated
        HashSet<Node> closedSet = new HashSet<Node>();
        // Those nodes to be evaluated (starting with the start position) sorted by the f-score
        SimplePriorityQueue<Node> openSet = new SimplePriorityQueue<Node>();
        openSet.Enqueue(graph.Nodes[startPosition], Vector2Int.Distance(startPosition, endPosition));
        // The map of navigated nodes
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        // The map of g scores
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        // Set the first score to zero
        gScore.Add(graph.Nodes[startPosition], 0);

        while(openSet.Count > 0)
        {
            // Take the top node and add it to the closed set (the priority queue means we get the node with lowest f score)
            Node current = openSet.Dequeue();
            closedSet.Add(current);
            if (current.Position == endPosition)
            {
                Path = ReconstructPath(cameFrom, current);
                return;
            }
            // Now go through each neighbour of the current node
            foreach (Edge neighbour in current.Edges)
            {
                // If we have already visited then skip this one
                if (closedSet.Contains(neighbour.Node)) continue;
                // Populate G Score if not already
                if(!gScore.ContainsKey(current))
                {
                    gScore.Add(current, float.MaxValue);
                }
                // Determine tentative g score
                float tentativeGScore = gScore[current] + neighbour.Cost;
                // Populate G Score if not already
                if (!gScore.ContainsKey(neighbour.Node))
                {
                    gScore.Add(neighbour.Node, float.MaxValue);
                }
                // If that tentative g score is less than that for the neighbour
                if (tentativeGScore < gScore[neighbour.Node])
                {
                    cameFrom.Add(neighbour.Node, current);
                    gScore[neighbour.Node] = tentativeGScore;
                    // If the neighbour is already in the open set then remove
                    if(openSet.Contains(neighbour.Node))
                    {
                        openSet.Remove(neighbour.Node);
                    }
                    // Then replace with a new version with an updated f score
                    openSet.Enqueue(neighbour.Node, tentativeGScore + Vector2Int.Distance(current.Position, neighbour.Node.Position));
                }
            }
        }
    }

    private Queue<Vector2Int> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        // Build the path using a linked list so that we can add to the beginning
        LinkedList<Vector2Int> totalPath = new LinkedList<Vector2Int>();
        totalPath.AddFirst(current.Position);
        // Loop through the cameFrom to build the path
        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.AddFirst(current.Position);
        }
        // Convert the list into a queue
        Queue<Vector2Int> totalQueue = new Queue<Vector2Int>();
        foreach(Vector2Int position in totalPath)
        {
            totalQueue.Enqueue(position);
        }
        // Remove the first item as it contains the current position
        totalQueue.Dequeue();
        // Return the fully constructed path
        return totalQueue;
    }

    public Vector2Int GetNextPosition()
    {
        // While there are items in the queue then send the next position
        if (Path.Count > 0)
        {
            return Path.Dequeue();
        }
        // If the queue is empty then return an "empty" vector2
        else
        {
            return Vector2Int.zero;
        }
    }
}
