using System;

[Serializable]
public class WorldSave
{
    public string Name;
    public int SizeX;
    public int SizeY;
    public WorldTileSave[] WorldTiles;
}