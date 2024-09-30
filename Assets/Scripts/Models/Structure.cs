using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure: TileType, IBuildableObject
{
    // Structure type is used to look up information from configuration etc.
    public StructureType StructureType { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsDoor { get; private set; }
    public bool IsWall { get; private set; }

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
        this.IsWall = config.IsWall;
        this.IsDoor = config.IsDoor;
        this.Tile = config.Tile;
    }
}
