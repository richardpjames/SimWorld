using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StructureDataConfiguration", menuName = "ScriptableObjects/Structure Data Configuration", order = 1)]
public class StructureDataConfiguration : ScriptableObject
{
    public StructureConfiguration[] configurations;
    private List<StructureConfiguration> configurationList = new List<StructureConfiguration>();
    // Start is called before the first frame update
    void OnEnable()
    {
        // Create a list from the configurations provided (for easier query)
        foreach (StructureConfiguration configuration in configurations)
        {
            configurationList.Add(configuration);
        }
    }

    public StructureConfiguration GetConfiguration(StructureType type)
    {
        // Query our list for an item which matches the properties of the structure 
        // i.e. the same name and connections
        StructureConfiguration match = configurationList.FirstOrDefault<StructureConfiguration>(c => c.StructureType == type);

        // Return the matched sprite from the above
        return match;
    }

    public TileBase GetTile(StructureType type)
    {
        // Query our list for an item which matches the properties of the structure 
        // i.e. the same name and connections
        StructureConfiguration match = configurationList.FirstOrDefault<StructureConfiguration>(c => c.StructureType == type);

        // Return the matched sprite from the above
        return match.Tile;   
    }

    [Serializable]
    public struct StructureConfiguration
    {
        public StructureType StructureType;
        public int Width;
        public int Height;
        public int MovementCost;
        public TileBase Tile;
        public bool IsWall;
        public bool IsDoor;
        public bool IsFloor;
    }
}
