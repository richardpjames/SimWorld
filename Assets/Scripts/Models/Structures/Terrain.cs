using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain : WorldTile
{
    public Terrain(string name, float movementCost, TileBase tile, Quaternion rotation, int width, int height, int buildCost, WorldLayer layer, bool buildingAllowed = true)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this._originalHeight = Height;
        this._originalWidth = Width;
        this.BuildCost = buildCost;
        this.Layer = layer;
        this.BuildingAllowed = buildingAllowed;
        this.Rotation = rotation;
        this.BuildMode = BuildMode.Single;
        this._canRotate = false;
    }

    // Terrain is valid in all positions, so always return true
    public override bool CheckValidity(World world, Vector2Int position)
    {
        return true;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Terrain(Name, MovementCost, Tile, Rotation, Width, Height, BuildCost, Layer, BuildingAllowed);
    }

}
