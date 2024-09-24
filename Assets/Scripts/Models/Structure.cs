using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure
{
    public string StructureType { get; private set; }
    public float MovementCost { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Tile BaseTile { get; private set; }

    public Structure(string structureType, float movementCost, int width, int height)
    {
        this.StructureType = structureType;
        this.MovementCost = movementCost;
        this.Width = width;
        this.Height = height;
    }
}
