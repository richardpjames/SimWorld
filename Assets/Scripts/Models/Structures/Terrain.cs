using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain : WorldTile
{
    public Terrain(string name, float movementCost, TileBase tile, int rotations, int width, int height, int buildCost, WorldLayer layer, bool buildingAllowed = true)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildTime = buildCost;
        this.Layer = layer;
        this.BuildingAllowed = buildingAllowed;
        this.Rotations = rotations;
        this.BuildMode = BuildMode.Single;
        this.CanRotate = false;
    }

    // Terrain is valid in all positions, so always return true
    public override bool CheckValidity(World world, Vector2Int position)
    {
        return true;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Terrain(Name, MovementCost, Tile, Rotations, Width, Height, BuildTime, Layer, BuildingAllowed);
    }

}
