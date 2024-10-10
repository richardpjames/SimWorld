using System.Collections.Generic;
using UnityEngine;

public class Node
{
    // This position in the world that this node corresponds to
    public Vector2Int Position { get; protected set; }
    // Each of the edges that lead out of this node
    public List<Edge> Edges { get; protected set; }
    public Node(Vector2Int position)
    {
        Edges = new List<Edge>();
        this.Position = position;
    }
    public void SetEdges(List<Edge> edges)
    {
        this.Edges = edges;
    }
}
