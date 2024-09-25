using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "FloorTileConfiguration", menuName = "ScriptableObjects/Floor Tile Configuration", order = 1)]
public class FloorTileConfiguration : ScriptableObject
{
    public TileConfiguration[] configurations;
    private List<TileConfiguration> configurationList;
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
    public Tile GetTile(FloorType type)
    {
        // Query our list for an item which matches the properties of the square 
        TileConfiguration match = configurationList.FirstOrDefault<TileConfiguration>(c => c.FloorType == type );
        // Return the matched sprite from the above
        return match.Tile;
    }

    [Serializable]
    public struct TileConfiguration
    {
        public FloorType FloorType;
        public Tile Tile;
    }
}
