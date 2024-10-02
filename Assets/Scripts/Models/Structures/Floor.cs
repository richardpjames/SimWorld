using UnityEngine;
using UnityEngine.Tilemaps;

public class Floor : WorldTile
{
    public Floor(string name, float movementCost, TileBase tile, int width, int height, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Floor;
        this.BuildingAllowed = true;
    }
}
