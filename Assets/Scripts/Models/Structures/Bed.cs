using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bed : WorldTile
{
    public Bed(string name, float movementCost, TileBase tile, int rotations, int width, int height, int buildTime, Dictionary<InventoryItem, int> cost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildTime = buildTime;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
        this.Rotations = rotations;
        this.BuildMode = BuildMode.Single;
        this.CanRotate = true;
        this.Cost = cost;
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
        return new Bed(Name, MovementCost, Tile, Rotations, Width, Height, BuildTime, Cost);
    }
}
