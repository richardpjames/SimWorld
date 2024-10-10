public class Edge
{
    // This is the cost to move along this edge
    public float Cost { get; protected set; }
    // This is the end node from the edge
    public Node Node { get; protected set; }
    public Edge(float cost, Node node)
    {
        this.Cost = cost;
        this.Node = node;
    }
}
