using UnityEngine;

public class Reserved : WorldTile
{
    public Reserved(WorldLayer layer)
    {
        this.Name = "Reserved";
        this.MovementCost = 1;
        this.Tile = null;
        this.Width = 1;
        this.Height = 1;
        this._originalHeight = Height;
        this._originalWidth = Width;
        this.BuildCost = 0;
        this.Layer = layer;
        this.BuildingAllowed = true;
        this.Rotation = Quaternion.identity;
    }

    public override bool CheckValidity(World world, Vector2Int position)
    {
        // Allowed on all tiles
        return true;
    }

    public override WorldTile NewInstance()
    {
        // Return a copy of the object as a new instance
        return new Reserved(Layer);
    }
}
