using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldTile
{
    public TileType Type { get; internal set; }
    public BuildMode BuildMode { get; internal set; }
    public WorldLayer Layer { get; internal set; }
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public int BuildTime { get; internal set; }
    public int CraftTime { get; internal set; }
    public string Name { get; internal set; }
    public float MovementCost { get; internal set; }
    public TileBase Tile { get; internal set; }
    public bool BuildingAllowed { get; internal set; }
    public int Rotations { get; internal set; } = 0;
    public Dictionary<InventoryItem, int> Cost { get; internal set; }
    public Dictionary<InventoryItem, int> Yield { get; internal set; }
    public Dictionary<InventoryItem, int> CraftCost { get; internal set; }
    public Dictionary<InventoryItem, int> CraftYield { get; internal set; }
    public bool Walkable { get => MovementCost != 0; }
    public bool Reserved { get; internal set; } = false;
    public Vector2Int BasePosition { get; internal set; }
    public Vector2Int WorkOffset { get; internal set; } = Vector2Int.zero;
    public bool CanRotate { get; internal set; }
    public bool RequiresUpdate { get; internal set; }
    public TileType HarvestType { get; internal set; }
    public JobQueue JobQueue { get; internal set; }
    public Guid CurrentJob { get; internal set; }
    public int JobCount { get; internal set; }
    public bool Continuous { get; internal set; }
    public World World { get; internal set; }
    public Inventory Inventory { get; internal set; }
    public float GrowthTime { get; internal set; }
    public WorldTile AdultTile { get; internal set; }
    public Vector2Int WorkPosition
    {
        get
        {
            if (BasePosition == null) return Vector2Int.zero;
            if (Rotations == 1) return BasePosition + new Vector2Int(-WorkOffset.x, WorkOffset.y);
            if (Rotations == 2) return BasePosition + new Vector2Int(-WorkOffset.x, -WorkOffset.y);
            if (Rotations == 3) return BasePosition + new Vector2Int(WorkOffset.x, -WorkOffset.y);
            // If no rotations then simply return the standard offset
            else return BasePosition + WorkOffset;
        }
    }
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
        bool requiresUpdate = false, TileType harvestType = TileType.Tree,
        string currentJob = "00000000-0000-0000-0000-000000000000", Dictionary<InventoryItem, int> craftCost = null,
        Dictionary<InventoryItem, int> craftYield = null, int craftTime = 0,
        int jobCount = 0, bool continuous = false, float growthTime = 0, WorldTile adultTile = null,
        Vector2Int workOffset = default)
    {
        //********************************************************************
        // Whenever adding a new field, be sure to update the NewInstance too!
        //********************************************************************
        this.JobQueue = GameObject.FindAnyObjectByType<JobQueue>(); ;
        this.World = GameObject.FindAnyObjectByType<World>();
        this.Inventory = GameObject.FindAnyObjectByType<Inventory>();

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
        this.CurrentJob = Guid.Parse(currentJob);
        this.CraftCost = craftCost;
        this.CraftYield = craftYield;
        this.CraftTime = craftTime;
        this.JobCount = jobCount;
        this.Continuous = continuous;
        this.GrowthTime = growthTime;
        this.AdultTile = adultTile;
        this.WorkOffset = workOffset;
    }

    public WorldTile NewInstance()
    {
        return new WorldTile(Type, BuildMode, Layer, Tile, Width, Height,
            BuildTime, Name, MovementCost, BuildingAllowed, Rotations,
            Cost, Yield, Reserved, CanRotate, RequiresUpdate, HarvestType,
            CurrentJob.ToString(), CraftCost, CraftYield, CraftTime,
            JobCount, Continuous, GrowthTime, AdultTile, WorkOffset);
    }
    public void Update(float delta)
    {
        // If no updates are required then exit
        if (!RequiresUpdate) return;
        // Updates for any saplings, crops etc.
        CropUpdater.Update(this, delta);
        // Updates for any harvesting or crafting tables
        CraftingUpdater.Update(this, delta);
    }
    public bool CheckValidity(World world, Vector2Int position)
    {
        if (Type == TileType.Terrain) return true;
        // Run through each of the validators and return false if they do
        if (!TileValidator.Validate(this, position)) return false;
        if (!DoorValidator.Validate(this, position)) return false;
        if (!FurnitureValidator.Validate(this, position)) return false;
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

    public WorldTileSave Serialize()
    {
        // Create a save object and populate
        WorldTileSave worldTileSave = new WorldTileSave();
        worldTileSave.Name = Name;
        worldTileSave.Rotations = Rotations;
        worldTileSave.BasePositionX = BasePosition.x;
        worldTileSave.BasePositionY = BasePosition.y;
        worldTileSave.Layer = (int) Layer;
        worldTileSave.GrowthTime = GrowthTime;
        worldTileSave.JobCount = JobCount;
        worldTileSave.Continuous = Continuous;
        worldTileSave.Type = (int)Type;
        worldTileSave.CurrentJob = CurrentJob;
        // Then return
        return worldTileSave;
    }
}
