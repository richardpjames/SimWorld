using UnityEngine.Tilemaps;

public abstract class TileType
{
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
}