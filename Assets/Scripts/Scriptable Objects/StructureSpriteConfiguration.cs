using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureSpriteConfiguration", menuName = "ScriptableObjects/Structure Sprite Configuration", order = 1)]
public class StructureSpriteConfiguration : ScriptableObject
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
    public Sprite GetSprite(Structure structure)
    {
        // Query our list for an item which matches the properties of the structure 
        // i.e. the same name and connections
        SpriteConfiguration match = configurationList.FirstOrDefault<SpriteConfiguration>(c => c.StructureType == structure.StructureType
                                                                                            && c.ConnectedNorth == structure.ConnectedNorth
                                                                                            && c.ConnectedEast == structure.ConnectedEast
                                                                                            && c.ConnectedSouth == structure.ConnectedSouth
                                                                                            && c.ConnectedWest == structure.ConnectedWest);
        // If no match then try again without the connection restrictions
        if (match.Sprite == null)
        {
            match = configurationList.FirstOrDefault<SpriteConfiguration>(c => c.StructureType == structure.StructureType);
        }
        // Return the matched sprite from the above
        return match.Sprite;
    }

    [Serializable]
    public struct SpriteConfiguration
    {
        public StructureType StructureType;
        public Sprite Sprite;
        public bool ConnectedNorth;
        public bool ConnectedEast;
        public bool ConnectedSouth;
        public bool ConnectedWest;
    }
}
