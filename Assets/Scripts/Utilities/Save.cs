using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Save
{
    // Data that we want to save
    public string WorldName;
    public int WorldWidth;
    public int WorldHeight;
    public List<SaveTile> Tiles;

    /// <summary>
    /// Creates the save game data from a world (saving the world and tiles)
    /// </summary>
    /// <param name="world">The world containing the tiles to be saved</param>
    public void PopulateWorld(World world)
    {
        // Set the basics about the world
        WorldName = world.Name;
        WorldWidth = world.Size.x;
        WorldHeight = world.Size.y;
        // Create a list of all tiles (which will need to hold their own coordinates)
        Tiles = new List<SaveTile>();
        // Loop through the world tiles and save their details
        for(int x = 0; x < WorldWidth; x++)
        {
            for(int y = 0; y < WorldHeight; y++)
            {
                // Create a new instance of the struct to hold the tiles
                SaveTile newTile = new SaveTile();
                // Set the property of each tile
                newTile.X = x;
                newTile.Y = y;
                // Set the tile type from the controller
                newTile.Type = WorldController.Instance.World.GetSquare(new Vector2Int(x,y)).TerrainType;
                // Default to no structures
                newTile.HasStructure = false;
                // Add any structures
                Structure structure = WorldController.Instance.World.GetSquare(new Vector2Int(x, y)).InstalledStructure;
                if (structure != null)
                {
                    newTile.HasStructure = true;
                    newTile.StructureType = structure.StructureType;
                    newTile.MovementCost = structure.MovementCost;
                    newTile.StructureWidth = structure.Width;
                    newTile.StructureHeight = structure.Height;
                }
                // Add this tile to the list
                Tiles.Add(newTile);
            }
        }
    }

    [Serializable]
    public struct SaveTile
    {
        // The tile itself
        public TerrainType Type;
        public int X;
        public int Y;
        // Any structure on the tile
        public bool HasStructure;
        public StructureType StructureType;
        public float MovementCost;
        public int StructureWidth;
        public int StructureHeight;
    }
}
