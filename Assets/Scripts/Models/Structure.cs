using UnityEngine;

public class Structure: TileType, IBuildableObject
{
    // Structure type is used to look up information from configuration etc.
    public StructureType StructureType { get; private set; }
    public float MovementCost { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public bool IsDoor { get; private set; }
    public bool IsWall { get; private set; }


    // Constructor takes all of the required information for creating a structure
    public Structure(StructureType structureType, float movementCost, int width, int height, bool isDoor = false, bool isWall = false)
    {
        this.StructureType = structureType;
        this.MovementCost = movementCost;
        this.Width = width;
        this.Height = height;
        this.IsDoor = isDoor;
        this.IsWall = IsWall;
    }

    /// <summary>
    /// Build a structure from a configuration provided by the StructureDataConfiguration scriptable object.
    /// </summary>
    /// <param name="config"></param>
    public Structure(StructureDataConfiguration.StructureConfiguration config)
    {
        this.StructureType = config.StructureType;
        this.MovementCost = config.MovementCost;
        this.Width = config.Width;
        this.Height = config.Height;
        this.IsDoor = config.IsDoor;
        this.IsWall = config.IsWall;
    }
}
