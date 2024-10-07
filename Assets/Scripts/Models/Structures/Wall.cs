using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : WorldTile
{
    public Wall(string name, float movementCost, TileBase tile, int rotations, int width, int height, int buildCost, Dictionary<InventoryItem, int> cost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this.BuildTime = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
        this.Rotations = rotations;
        this.BuildMode = BuildMode.Line;
        this.CanRotate = false;
        this.Cost = cost;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Wall(Name, MovementCost, Tile, Rotations, Width, Height, BuildTime, Cost);
    }
}
