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

    public WorldTile Deserialize()
    {
        // Create a new instance from the prefab factory
        PrefabFactory prefabFactory = GameObject.FindAnyObjectByType<PrefabFactory>();
        WorldTile tile = prefabFactory.Create(Name);
        // Set the number of rotations (everything else dealt with by the factory or the world)
        tile.Rotations = Rotations;
        // Return the tile
        return tile;
    }
}