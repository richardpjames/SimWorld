using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StructureTileConfiguration", menuName = "ScriptableObjects/Structure Tile Configuration", order = 1)]
public class StructureTileConfiguration : ScriptableObject
{
    public TileConfiguration[] configurations;
    private List<TileConfiguration> configurationList = new List<TileConfiguration>();
    // Start is called before the first frame update
    void OnEnable()
    {
        // Create a list from the configurations provided (for easier query)
        foreach (TileConfiguration configuration in configurations)
        {
            configurationList.Add(configuration);
        }
    }

    // Update is called once per frame
    public TileBase GetTile(StructureType type)
    {
        // Query our list for an item which matches the properties of the structure 
        TileConfiguration match = configurationList.FirstOrDefault<TileConfiguration>(c => c.StructureType == type);
        // Return the matched sprite from the above
        return match.Tile;
    }

    [Serializable]
    public struct TileConfiguration
    {
        public StructureType StructureType;
        public TileBase Tile;
    }
}
