using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorDataConfiguration", menuName = "ScriptableObjects/Floor Data Configuration", order = 1)]
public class FloorDataConfiguration : ScriptableObject
{
    public FloorConfiguration[] configurations;
    private List<FloorConfiguration> configurationList = new List<FloorConfiguration>();
    // Start is called before the first frame update
    void OnEnable()
    {
        // Create a list from the configurations provided (for easier query)
        foreach (FloorConfiguration configuration in configurations)
        {
            configurationList.Add(configuration);
        }
    }

    // Update is called once per frame
    public FloorConfiguration GetConfiguration(FloorType type)
    {
        // Query our list for an item which matches the properties of the floor 
        // i.e. the same name
        FloorConfiguration match = configurationList.FirstOrDefault<FloorConfiguration>(c => c.FloorType == type);

        // Return the matched sprite from the above
        return match;
    }

    [Serializable]
    public struct FloorConfiguration
    {
        public FloorType FloorType;
        public int MovementCost;
    }
}
