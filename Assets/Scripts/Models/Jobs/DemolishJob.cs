using UnityEngine;

public class DemolishJob : Job
{
    public DemolishJob(World world, Vector2Int position, WorldLayer layer)
    {
        WorldTile worldTile = world.GetWorldTile(position, layer);
        this.Position = position;
        this.JobCost = worldTile.BuildTime;
        this.Indicator = worldTile.Tile;
        this.Layer = layer;
        this.Complete = false;
        this.OnJobComplete += (job) => { world.RemoveWorldTile(position, layer); };
        this.Rotation = Quaternion.identity;

        // Reserve the tile to stop other jobs
        if (world.GetWorldTile(position, layer) != null)
        {
            world.GetWorldTile(position, layer).Reserved = true;
        }
    }
}