using System;
using UnityEngine;

[Serializable]
public class WorldTileSave
{
    public int BasePositionX;
    public int BasePositionY;
    public int Layer;
    public string Name;
    public int Rotations;
    public float GrowthTime;
    public bool Continuous;
    public int JobCount;

    public WorldTile Deserialize()
    {
        // Create a new instance from the prefab factory
        PrefabFactory prefabFactory = GameObject.FindAnyObjectByType<PrefabFactory>();
        WorldTile tile = prefabFactory.Create(Name);
        // Set the number of rotations (everything else dealt with by the factory or the world)
        tile.Rotations = Rotations;
        // Set the growth time for crops etc.
        tile.GrowthTime = GrowthTime;
        // Details for generating jobs
        tile.Continuous = Continuous;
        tile.JobCount = JobCount;
        // Return the tile
        return tile;
    }
}