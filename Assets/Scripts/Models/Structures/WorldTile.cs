using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class WorldTile
{
    public WorldLayer Layer { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int BuildCost { get; protected set; }
    public string Name { get; protected set; }
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
    public bool BuildingAllowed { get; protected set; }

    public abstract bool CheckValidity(World world, Vector2Int position);
}
