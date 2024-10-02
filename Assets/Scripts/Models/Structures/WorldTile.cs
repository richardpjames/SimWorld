using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class WorldTile
{
    public BuildMode BuildMode { get; protected set; }
    public Quaternion Rotation { get; protected set; }
    public WorldLayer Layer { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int BuildCost { get; protected set; }
    public string Name { get; protected set; }
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
    public bool BuildingAllowed { get; protected set; }
    public bool Walkable { get => MovementCost == 0; }
    public bool Reserved = false;
    public Vector2Int BasePosition;
    protected bool _canRotate;

    // For calculating the extremes of the shape
    public int MinX { get { if (Width >= 0) { return 0; } else { return Width + 1; } } }
    public int MinY { get { if (Height >= 0) { return 0; } else { return Height + 1; } } }
    public int MaxX { get { if (Width <= 0) { return 1; } else { return Width; } } }
    public int MaxY { get { if (Height <= 0) { return 1; } else { return Height; } } }

    protected int _originalHeight;
    protected int _originalWidth;

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
        if (!_canRotate) return;
        // Rotate the item by 90 degrees
        Rotation *= Quaternion.Euler(0, 0, -90f);

        // First rotation (90 degrees)
        if (Width == _originalWidth && Height == _originalHeight)
        {
            Width = _originalHeight;
            Height = _originalWidth;
        }
        // Second rotations (180 degrees)
        else if (Width == _originalHeight && Height == _originalWidth)
        {
            Width = _originalWidth;
            Height = -_originalHeight;
        }
        // Third Rotation (270 degrees)
        else if (Width == _originalWidth && Height == -_originalHeight)
        {
            Width = -_originalHeight;
            Height = _originalWidth;
        }
        // Four rotations (0 degrees)
        else if (Width == -_originalHeight && Height == _originalWidth)
        {
            Width = _originalWidth;
            Height = _originalHeight;
        }
    }

    public virtual WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return null;
    }
}
