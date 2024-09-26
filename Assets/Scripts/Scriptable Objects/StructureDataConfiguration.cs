using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // Update is called once per frame
    public StructureConfiguration GetConfiguration(StructureType type)
    {
        // Query our list for an item which matches the properties of the structure 
        // i.e. the same name and connections
        StructureConfiguration match = configurationList.FirstOrDefault<StructureConfiguration>(c => c.StructureType == type);

        // Return the matched sprite from the above
        return match;
    }

    [Serializable]
    public struct StructureConfiguration
    {
        public StructureType StructureType;
        public int Width;
        public int Height;
        public int MovementCost;
    }
}
