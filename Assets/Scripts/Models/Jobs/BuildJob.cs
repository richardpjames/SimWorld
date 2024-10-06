using UnityEngine;

public class BuildJob : Job
{
    public BuildJob(World world, Vector2Int position, WorldTile worldTile)
    {
        this.Position = position;
        this.JobCost = worldTile.BuildTime;
        this.Indicator = worldTile.Tile;
        this.Layer = worldTile.Layer;
        this.Complete = false;
        this.OnJobComplete += (job) => { world.UpdateWorldTile(position, worldTile); };
        this.Rotation = worldTile.Rotation;
        this.WorldTile = worldTile;

        // If this isn't valid, then immediately complete the job and exit
        if (worldTile.CheckValidity(world, position) == false)
        {
            Complete = true;
            return;
        }
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + worldTile.MinX);
        int maxX = Mathf.Max(position.x, position.x + worldTile.MaxX);
        int minY = Mathf.Min(position.y, position.y + worldTile.MinY);
        int maxY = Mathf.Max(position.y, position.y + worldTile.MaxY);
        // Create a number of reserved tiles
        for (int x = minX; x < maxX; x++)
        {
            // Create a number of reserved tiles
            for (int y = minY; y < maxY; y++)
            {
                world.UpdateWorldTile(new Vector2Int(x, y), new Reserved(worldTile.Layer));
            }
        }
    }
}