using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain : WorldTile
{
    public Terrain(string name, float movementCost, TileBase tile, int width, int height, int buildCost, WorldLayer layer, bool buildingAllowed = true)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildCost = buildCost;
        this.Layer = layer;
        this.BuildingAllowed = buildingAllowed;
    }

    // Terrain is valid in all positions, so always return true
    public override bool CheckValidity(World world, Vector2Int position)
    {
        return true;
    }
}
