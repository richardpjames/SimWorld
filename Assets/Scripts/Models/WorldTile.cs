using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldTile
{
    public TileType Type { get; protected set; }
    public BuildMode BuildMode { get; protected set; }
    public WorldLayer Layer { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int BuildTime { get; protected set; }
    public int CraftTime { get; protected set; }
    public string Name { get; protected set; }
    public float MovementCost { get; protected set; }
    public TileBase Tile { get; protected set; }
    public bool BuildingAllowed { get; protected set; }
    public int Rotations { get; protected set; } = 0;
    public Dictionary<InventoryItem, int> Cost { get; protected set; }
    public Dictionary<InventoryItem, int> Yield { get; protected set; }
    public Dictionary<InventoryItem, int> CraftCost { get; protected set; }
    public Dictionary<InventoryItem, int> CraftYield { get; protected set; }
    public bool Walkable { get => MovementCost != 0; }
    public bool Reserved { get; set; } = false;
    public Vector2Int BasePosition { get; set; }
    public bool CanRotate { get; protected set; }
    public bool RequiresUpdate { get; protected set; }
    public TileType HarvestType { get; protected set; }
    public JobQueue JobQueue { get; protected set; }
    public Job CurrentJob { get; protected set; }
    public int JobCount { get; protected set; }
    public bool Continuous { get; protected set; }
    public World World { get; protected set; }
    public Inventory Inventory { get; protected set; }
    public float GrowthTime { get; protected set; }
    public WorldTile AdultTile { get; protected set; }
    public Quaternion Rotation
    {
        get
        {
            if (Rotations == 1) return Quaternion.Euler(0, 0, -90f);
            else if (Rotations == 2) return Quaternion.Euler(0, 0, -180f);
            else if (Rotations == 3) return Quaternion.Euler(0, 0, -270f);
            else return Quaternion.identity;
        }
    }
    public int RotationAdjustedWidth
    {
        get
        {
            if (Rotations == 1) return Height;
            else if (Rotations == 2) return -Width;
            else if (Rotations == 3) return -Height;
            else return Width;
        }
    }
    public int RotationAdjustedHeight
    {
        get
        {
            if (Rotations == 1) return -Width;
            else if (Rotations == 2) return -Height;
            else if (Rotations == 3) return Width;
            else return Height;
        }
    }
    public int MinX { get { if (RotationAdjustedWidth >= 0) { return 0; } else { return RotationAdjustedWidth + 1; } } }
    public int MinY { get { if (RotationAdjustedHeight >= 0) { return 0; } else { return RotationAdjustedHeight + 1; } } }
    public int MaxX { get { if (RotationAdjustedWidth <= 0) { return 1; } else { return RotationAdjustedWidth; } } }
    public int MaxY { get { if (RotationAdjustedHeight <= 0) { return 1; } else { return RotationAdjustedHeight; } } }

    public Action<WorldTile> OnWorldTileUpdated;

    public WorldTile(TileType type, BuildMode buildMode, WorldLayer layer,
        TileBase tile, int width = 1, int height = 1, int buildTime = 0,
        string name = "", float movementCost = 1,
        bool buildingAllowed = true, int rotations = 0, Dictionary<InventoryItem, int> cost = null,
        Dictionary<InventoryItem, int> yield = null, bool reserved = false, bool canRotate = false,
        bool requiresUpdate = false, TileType harvestType = TileType.Tree, JobQueue jobQueue = null,
        Job currentJob = null, World world = null, Dictionary<InventoryItem, int> craftCost = null,
        Dictionary<InventoryItem, int> craftYield = null, int craftTime = 0, Inventory inventory = null,
        int jobCount = 0, bool continuous = false, float growthTime = 0, WorldTile adultTile = null)
    {
        //********************************************************************
        // Whenever adding a new field, be sure to update the NewInstance too!
        //********************************************************************
        this.Type = type;
        this.BuildMode = buildMode;
        this.Layer = layer;
        this.Width = width;
        this.Height = height;
        this.BuildTime = buildTime;
        this.Name = name;
        this.MovementCost = movementCost;
        this.Tile = tile;
        this.BuildingAllowed = buildingAllowed;
        this.Rotations = rotations;
        this.Cost = cost;
        this.Yield = yield;
        this.Reserved = reserved;
        this.CanRotate = canRotate;
        this.RequiresUpdate = requiresUpdate;
        this.HarvestType = harvestType;
        this.JobQueue = jobQueue;
        this.CurrentJob = currentJob;
        this.World = world;
        this.CraftCost = craftCost;
        this.CraftYield = craftYield;
        this.CraftTime = craftTime;
        this.Inventory = inventory;
        this.JobCount = jobCount;
        this.Continuous = continuous;
        this.GrowthTime = growthTime;
        this.AdultTile = adultTile;
    }

    public WorldTile NewInstance()
    {
        return new WorldTile(Type, BuildMode, Layer, Tile, Width, Height,
            BuildTime, Name, MovementCost, BuildingAllowed, Rotations,
            Cost, Yield, Reserved, CanRotate, RequiresUpdate, HarvestType,
            JobQueue, CurrentJob, World, CraftCost, CraftYield, CraftTime, Inventory,
            JobCount, Continuous, GrowthTime, AdultTile);
    }
    public void Update(float delta)
    {
        if (!RequiresUpdate) return;
        // For saplings etc. which will grow into full grown plants/trees
        if(Type == TileType.Sapling)
        {
            // Count down until the end of the growth lifetime
            GrowthTime = GrowthTime -delta;
            // When the time is up, we destory this tile and create an adult version
            if(GrowthTime < 0)
            {
                // Remove this tile from the world
                World.RemoveWorldTile(BasePosition, Layer);
                // Add the adult tile to the world
                World.UpdateWorldTile(BasePosition, AdultTile);
            }
        }
        // For harvesters like the woodcutters table
        if (Type == TileType.HarvestersTable || Type == TileType.CraftersTable)
        {
            // If no jobs have been created, or the current job is complete
            if ((CurrentJob == null || CurrentJob.Complete) && (JobCount > 0 || Continuous))
            {
                // Create a new job to harvest the resource
                if (Type == TileType.HarvestersTable)
                {
                    // Get the nearest structure we want to harvest but return if there are none
                    Vector2Int resourcePosition = World.GetNearestStructure(BasePosition, HarvestType);
                    if (resourcePosition == null) return;
                    // Otherwise get the tile
                    WorldTile resourceTile = World.GetWorldTile(resourcePosition, WorldLayer.Structure);
                    if (resourceTile == null) return;
                    CurrentJob = Job.HarvestJob(World, BasePosition, this, resourcePosition, resourceTile);
                }
                else if (Type == TileType.CraftersTable)
                {
                    CurrentJob = Job.CraftJob(World, BasePosition, this, Inventory);
                }
                // If we have been able to create a new job
                if (CurrentJob != null)
                {
                    if(!Continuous) JobCount--;
                    JobQueue.Add(CurrentJob);
                    OnWorldTileUpdated?.Invoke(this);
                }
            }
        }
    }
    public bool CheckValidity(World world, Vector2Int position)
    {
        if (Type == TileType.Terrain) return true;
        // Find the min and max X and Y values to loop between
        int minX = Mathf.Min(position.x, position.x + MinX);
        int maxX = Mathf.Max(position.x, position.x + MaxX);
        int minY = Mathf.Min(position.y, position.y + MinY);
        int maxY = Mathf.Max(position.y, position.y + MaxY);
        // Checks for all tiles within the object
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);
                // Check if we are building on allowable terrain
                if (!world.IsBuildable(checkPosition)) return false;
                WorldTile worldTile = world.GetWorldTile(checkPosition, Layer);
                if (worldTile != null) return false;
                // If we are out of bounds then don't build
                if (!world.CheckBounds(checkPosition)) return false;
            }
        }
        if (Type == TileType.Door)
        {
            // Must be walls above and below or side to side, but no more than that
            WorldTile wallAbove = world.GetWorldTile(position + Vector2Int.up, WorldLayer.Structure);
            WorldTile wallBelow = world.GetWorldTile(position + Vector2Int.down, WorldLayer.Structure);
            WorldTile wallLeft = world.GetWorldTile(position + Vector2Int.left, WorldLayer.Structure);
            WorldTile wallRight = world.GetWorldTile(position + Vector2Int.right, WorldLayer.Structure);
            // Determine whether there is a wall above, below, left and right
            bool isWallAbove = wallAbove != null && wallAbove.Type == TileType.Wall;
            bool isWallBelow = wallBelow != null && wallBelow.Type == TileType.Wall;
            bool isWallLeft = wallLeft != null && wallLeft.Type == TileType.Wall;
            bool isWallRight = wallRight != null && wallRight.Type == TileType.Wall;
            // If no valid combination (above and below) or (left and right)
            if (!((isWallAbove && isWallBelow) || (isWallLeft && isWallRight))) return false;
            // If above and below, but there are also left or right
            if ((isWallAbove && isWallBelow) && (isWallLeft || isWallRight)) return false;
            // If left and right, but there are also above or below
            if ((isWallLeft && isWallRight) && (isWallAbove || isWallBelow)) return false;
            // Having checked the walls, rotate accordingly
            if (isWallAbove)
            {
                Rotations = 0;
            }
            if (isWallLeft)
            {
                Rotations = 1;
            }
        }
        if (Type == TileType.Bed || Type == TileType.Chair || Type == TileType.Table)
        {
            // This must be placed inside
            if (!world.IsInside(position)) return false;
        }
        // Otherwise return true
        return true;
    }

    public void Rotate()
    {
        // Return if not allowed to rotate
        if (!CanRotate) return;
        // Otherwise increase rotations to a max of 3
        Rotations++;
        // If we have already rotated three times then return to the original
        if (Rotations > 3) Rotations = 0;
    }

    // For updating the job count through the UI
    public void SetJobCount(int count)
    {
        JobCount = count;
        OnWorldTileUpdated?.Invoke(this);
    }

    // For toggling whether the tile is continuous
    public void SetContinuous(bool value)
    {
        Continuous = value;
        OnWorldTileUpdated?.Invoke(this);
    }

}
