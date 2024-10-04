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
        this.BuildingAllowed = true;
        this.Rotation = rotation;
        this.BuildMode = BuildMode.Single;
        this._canRotate = true;
    }

    public override bool CheckValidity(World world, Vector2Int position)
    {
        // Check the base validity checks
        if (!base.CheckValidity(world, position)) return false;
        // This must be placed inside
        if (!world.IsInside(position)) return false;
        // Otherwise return true
        return true;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Bed(Name, MovementCost, Tile, Rotation, Width, Height, BuildCost);
    }
}
