using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainSpriteConfiguration", menuName = "ScriptableObjects/Terrain Sprite Configuration", order = 1)]
public class TerrainSpriteConfiguration : ScriptableObject
{
    public SpriteConfiguration[] configurations;
    private List<SpriteConfiguration> configurationList;
    // Start is called before the first frame update
    void OnEnable()
    {
        // Create a list from the configurations provided (for easier query)
        foreach (SpriteConfiguration configuration in configurations)
        {
            configurationList.Add(configuration);
        }
    }

    // Update is called once per frame
    public Sprite GetSprite(Square square)
    {
        // Query our list for an item which matches the properties of the square 
        SpriteConfiguration match = configurationList.FirstOrDefault<SpriteConfiguration>(c => c.Terrain == square.TerrainType );
        // Return the matched sprite from the above
        return match.Sprite;
    }

    [Serializable]
    public struct SpriteConfiguration
    {
        public TerrainType Terrain;
        public Sprite Sprite;
    }
}
