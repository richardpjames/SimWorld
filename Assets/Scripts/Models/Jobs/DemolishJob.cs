using UnityEngine;

public class DemolishJob : Job
{
    public DemolishJob(World world, Vector2Int position, WorldLayer layer)
    {
        WorldTile worldTile = world.GetWorldTile(position, layer);
        this.Position = position;
        this.JobCost = worldTile.BuildCost;
        this.Indicator = worldTile.Tile;
        this.Layer = layer;
        this.Complete = false;
        this.OnJobComplete += (job) => { world.RemoveWorldTile(position, layer); };
    }
}