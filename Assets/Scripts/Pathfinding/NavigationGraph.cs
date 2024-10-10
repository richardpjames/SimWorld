using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationGraph
{
    public Dictionary<Vector2Int, Node> Nodes { get; protected set; }
    private bool calculating = false;

    public async void Calculate(World world)
    {
        // Prevent multiple threads from running
        if (!calculating)
        {
            calculating = true;
            await Task.Run(() => AsyncCalculate(world));
        }
    }
    private void AsyncCalculate(World world)
    {
        Dictionary<Vector2Int, Node> tempNodes = new Dictionary<Vector2Int, Node>();

        // Create nodes for each tile in the world
        for (int x = 0; x < world.Size.x; x++)
        {
            for (int y = 0; y < world.Size.y; y++)
            {
                Vector2Int loopPosition = new Vector2Int(x, y);
                tempNodes.Add(loopPosition, new Node(loopPosition));
            }
        }
        // Now create the edges between each node
        foreach (Vector2Int nodePosition in tempNodes.Keys)
        {
            // Create a list of all possible neighbours (8 in total)
            List<Vector2Int> neighbours = new List<Vector2Int>();
            //All eight variations
            neighbours.Add(nodePosition + Vector2Int.up);
            neighbours.Add(nodePosition + Vector2Int.up + Vector2Int.right);
            neighbours.Add(nodePosition + Vector2Int.right);
            neighbours.Add(nodePosition + Vector2Int.right + Vector2Int.down);
            neighbours.Add(nodePosition + Vector2Int.down);
            neighbours.Add(nodePosition + Vector2Int.down + Vector2Int.left);
            neighbours.Add(nodePosition + Vector2Int.left);
            neighbours.Add(nodePosition + Vector2Int.left + Vector2Int.up);
            // Create a list of edges from the neighbours
            List<Edge> edges = new List<Edge>();
            foreach (Vector2Int neighbour in neighbours)
            {
                // If this is walkable and in bounds of our graph
                if (tempNodes.ContainsKey(neighbour))
                {
                    edges.Add(new Edge(world.MovementCost(neighbour), tempNodes[neighbour]));
                }
            }
            // Add all of the edges to the node
            tempNodes[nodePosition].SetEdges(edges);
        }
        // Swap the current nodes for the calculated values
        Nodes = tempNodes;
        // Confirm that the current calculation is complete
        calculating = false;
    }

}
