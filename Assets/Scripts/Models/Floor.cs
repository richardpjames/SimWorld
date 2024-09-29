using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Floor : TileType, IBuildableObject
{
    public FloorType FloorType { get; private set; }

    // Constructor takes all of the required information for creating a floor
    public Floor(FloorType floorType, float movementCost, TileBase tile)
    {
        this.FloorType = floorType;
        this.MovementCost = movementCost;
        this.Tile = tile;
    }

    /// <summary>
    /// Build a floor from a configuration provided by the FloorDataConfiguration scriptable object.
    /// </summary>
    /// <param name="config"></param>
    public Floor(FloorDataConfiguration.FloorConfiguration config)
    {
        this.FloorType = config.FloorType;
        this.MovementCost = config.MovementCost;
        this.Tile = config.Tile;
    }
}
