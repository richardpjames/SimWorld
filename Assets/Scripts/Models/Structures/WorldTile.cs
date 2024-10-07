using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class WorldTile
{
    public BuildMode BuildMode { get; protected set; }
    // Determine the quaternion from the number of rotations
    public Quaternion Rotation
    {
        get
        {
            if (Rotations == 1) return Quaternion.Euler(0, 0, -90f);
            else if (Rotations == 2) return Quaternion.Euler(0, 0, -180f);
            else if (Rotations == 3) return Quaternion.Euler(0, 0, -270f);
            else return Quaternion.identity;
        }
    }
    public WorldLayer Layer { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int RotationAdjustedWidth
    {
        get
        {
            if (Rotations == 1) return Height;
            else if (Rotations == 2) return Width;
            else if (Rotations == 3) return -Height;
            else return Width;
        }
    }
    public int RotationAdjustedHeight
    {
        get
        {
            if (Rotations == 1) return Width;
            else if (Rotations == 2) return -Height;
            else if (Rotations == 3) return Width;
            else return Height;
        }
    }
    public int BuildTime { get; protected set; }
    public string Name { get; protected set; }
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
    public bool BuildingAllowed { get; protected set; }
    public int Rotations { get; protected set; } = 0;
    public Dictionary<InventoryItem, int> Cost { get; protected set; }
    public Dictionary<InventoryItem, int> Yield { get; protected set; }
    public bool Walkable { get => MovementCost != 0; }
    public bool Reserved { get; set; } = false;
    public Vector2Int BasePosition { get; set; }
    public bool CanRotate { get; protected set; }

    // For calculating the extremes of the shape
    public int MinX { get { if (RotationAdjustedWidth >= 0) { return 0; } else { return RotationAdjustedWidth + 1; } } }
    public int MinY { get { if (RotationAdjustedHeight >= 0) { return 0; } else { return RotationAdjustedHeight + 1; } } }
    public int MaxX { get { if (RotationAdjustedWidth <= 0) { return 1; } else { return RotationAdjustedWidth; } } }
    public int MaxY { get { if (RotationAdjustedHeight <= 0) { return 1; } else { return RotationAdjustedHeight; } } }

    public virtual bool CheckValidity(World world, Vector2Int position)
    {
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + MinX);
        int maxX = Mathf.Max(position.x, position.x + MaxX);
        int minY = Mathf.Min(position.y, position.y + MinY);
        int maxY = Mathf.Max(position.y, position.y + MaxY);
        // Checks for all tiles within the object
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);
                // Check if we are building on allowable terrain
                if (!world.IsBuildable(checkPosition)) return false;
                WorldTile worldTile = world.GetWorldTile(checkPosition, Layer);
                if (worldTile != null) return false;
                // If we are out of bounds then don't build
                if (!world.CheckBounds(checkPosition)) return false;
            }
        }
        // Otherwise return true
        return true;
    }

    public virtual void Rotate()
    {
        // Return if not allowed to rotate
        if (!CanRotate) return;
        // Otherwise increase rotations to a max of 3
        Rotations++;
        // If we have already rotated three times then return to the original
        if (Rotations > 3) Rotations = 0;
    }

    public virtual WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return null;
    }
}
