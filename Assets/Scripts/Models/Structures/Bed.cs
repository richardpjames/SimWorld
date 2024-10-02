using UnityEngine;
using UnityEngine.Tilemaps;

public class Bed : WorldTile
{
    public Bed(string name, float movementCost, TileBase tile, Quaternion rotation, int width, int height, int buildCost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this._originalHeight = Height;
        this._originalWidth = Width;
        this.BuildCost = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = false;
        this.Rotation = rotation;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Bed(Name, MovementCost, Tile, Rotation, Width, Height, BuildCost);
    }
}
