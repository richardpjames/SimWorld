using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : WorldTile
{
    public Wall(string name, float movementCost, TileBase tile, Quaternion rotation, int width, int height, int buildCost, Dictionary<InventoryItem, int> cost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = width;
        this.Height = height;
        this._originalHeight = Height;
        this._originalWidth = Width;
        this.BuildTime = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
        this.Rotation = rotation;
        this.BuildMode = BuildMode.Line;
        this._canRotate = false;
        this.Cost = cost;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Wall(Name, MovementCost, Tile, Rotation, Width, Height, BuildTime, Cost);
    }
}
