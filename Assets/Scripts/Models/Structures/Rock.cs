using UnityEngine;
using UnityEngine.Tilemaps;

public class Rock : WorldTile
{
    public Rock(string name, float movementCost, TileBase tile, int width, int height, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = false;
    }
}
