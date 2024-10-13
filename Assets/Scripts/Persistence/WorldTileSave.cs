using System;
using UnityEngine;

[Serializable]
public class WorldTileSave
{
    public int Type;
    public int BasePositionX;
    public int BasePositionY;
    public int Layer;
    public string Name;
    public int Rotations;
    public float GrowthTime;
    public bool Continuous;
    public int JobCount;
    public Guid CurrentJob;

    public WorldTile Deserialize()
    {
        // Create a new instance from the prefab factory
        PrefabFactory prefabFactory = GameObject.FindAnyObjectByType<PrefabFactory>();
        WorldTile tile = null;
        if (Type != (int)TileType.Reserved)
        {
            tile = prefabFactory.Create(Name);
        }
        else
        {
            tile = prefabFactory.CreateReserved((WorldLayer)Layer);
        }
        // Set the number of rotations (everything else dealt with by the factory or the world)
        tile.Rotations = Rotations;
        // Set the growth time for crops etc.
        tile.GrowthTime = GrowthTime;
        // Details for generating jobs
        tile.Continuous = Continuous;
        tile.JobCount = JobCount;
        tile.CurrentJob = CurrentJob;
        // Return the tile
        return tile;
    }
}