using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tree : WorldTile
{
    public Tree(string name, float movementCost, TileBase tile, int rotations, int width, int height, int buildCost, Dictionary<InventoryItem, int> yield)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildTime = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = false;
        this.Rotations = rotations;
        this.BuildMode = BuildMode.Single;
        this.CanRotate = false;
        this.Yield = yield;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Tree(Name, MovementCost, Tile, Rotations, Width, Height, BuildTime, Yield);
    }
}
