using UnityEngine;

public class BuildJob : Job
{
    WorldTile worldTile;

    public BuildJob(World world, Vector2Int position, WorldTile worldTile)
    {
        this.Position = position;
        this.JobCost = worldTile.BuildCost;
        this.Indicator = worldTile.Tile;
        this.Layer = worldTile.Layer;
        this.Complete = false;
        this.OnJobComplete += (job) => { world.UpdateWorldTile(position, worldTile); };
    }
}