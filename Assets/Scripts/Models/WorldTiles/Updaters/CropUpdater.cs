public static class CropUpdater
{
    public static void Update(WorldTile tile, World world, float delta)
    {
        // For saplings etc. which will grow into full grown plants/trees
        if (tile.Type == TileType.Sapling)
        {
            // Count down until the end of the growth lifetime
            tile.GrowthTime = tile.GrowthTime - delta;
            // When the time is up, we destory this tile and create an adult version
            if (tile.GrowthTime < 0)
            {
                // Remove this tile from the world
                world.RemoveWorldTile(tile.BasePosition, tile.Layer);
                // Add the adult tile to the world
                world.UpdateWorldTile(tile.BasePosition, tile.AdultTile);
            }
        }
    }
}
