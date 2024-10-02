using UnityEngine;
using UnityEngine.Tilemaps;

public class Rock : WorldTile
{
    public Rock(string name, float movementCost, TileBase tile, Quaternion rotation, int width, int height, int buildCost)
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
        this.BuildMode = BuildMode.Single;
        this._canRotate = false;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Rock(Name, MovementCost, Tile, Rotation, Width, Height, BuildCost);
    }

}
