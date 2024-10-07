using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : WorldTile
{
    public Door(string name, float movementCost, TileBase tile, int rotations, int buildCost, Dictionary<InventoryItem, int> cost)
    {
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.Width = 1;
        this.Height = 1;
        this.BuildTime = buildCost;
        this.Layer = WorldLayer.Structure;
        this.BuildingAllowed = true;
        this.Rotations = rotations;
        this.BuildMode = BuildMode.Single;
        this.CanRotate = false;
        this.Cost = cost;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Door(Name, MovementCost, Tile, Rotations, BuildTime, Cost);
    }

    public override bool CheckValidity(World world, Vector2Int position)
    {
        if(!base.CheckValidity(world, position)) return false;
        // Must be walls above and below or side to side, but no more than that
        WorldTile wallAbove = world.GetWorldTile(position + Vector2Int.up, WorldLayer.Structure);
        WorldTile wallBelow = world.GetWorldTile(position + Vector2Int.down, WorldLayer.Structure);
        WorldTile wallLeft = world.GetWorldTile(position + Vector2Int.left, WorldLayer.Structure);
        WorldTile wallRight = world.GetWorldTile(position + Vector2Int.right, WorldLayer.Structure);
        // Determine whether there is a wall above, below, left and right
        bool isWallAbove = wallAbove != null && wallAbove.GetType() == typeof(Wall);
        bool isWallBelow = wallBelow != null && wallBelow.GetType() == typeof(Wall);
        bool isWallLeft = wallLeft != null && wallLeft.GetType() == typeof(Wall);
        bool isWallRight = wallRight != null && wallRight.GetType() == typeof(Wall);
        // If no valid combination (above and below) or (left and right)
        if (!((isWallAbove && isWallBelow) || (isWallLeft && isWallRight))) return false;
        // If above and below, but there are also left or right
        if((isWallAbove && isWallBelow) && (isWallLeft || isWallRight)) return false;
        // If left and right, but there are also above or below
        if ((isWallLeft && isWallRight) && (isWallAbove || isWallBelow)) return false;
        // Having checked the walls, rotate accordingly
        if(isWallAbove)
        {
            Rotations = 0;
        }
        if(isWallLeft)
        {
            Rotations = 1;
        }
        // Otherwise return true
        return true;
    }
}
