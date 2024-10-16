using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobStep
{
    public Guid Guid { get; internal set; }
    public JobType Type { get; protected set; }
    public World World { get; protected set; }
    public WorldTile WorldTile { get; protected set; }
    public Vector2Int Position { get; protected set; }
    public float Cost { get; protected set; }
    public bool Complete { get; protected set; }
    public TileBase Indicator { get => WorldTile.Tile; }
    public int Rotations { get; internal set; }
    public Inventory Inventory { get; protected set; }
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

    public Action<JobStep> OnJobStepComplete;

    // FIXME: Doesn't always need the inventory, so add a check and remove unneeded references
    public JobStep(JobType type, WorldTile worldTile, Vector2Int position, float cost, bool complete, int rotations)
    {
        // Create a guid
        Guid = Guid.NewGuid();
        World = GameObject.FindAnyObjectByType<World>();
        Type = type;
        WorldTile = worldTile;
        Position = position;
        Cost = cost;
        Complete = complete;
        Rotations = rotations;
        Inventory = GameObject.FindAnyObjectByType<Inventory>();

        if (Type == JobType.Build) OnJobStepComplete += (JobStep) => { WorldTile.Rotations = JobStep.Rotations; World.UpdateWorldTile(Position, WorldTile); };
        if (Type == JobType.Demolish) OnJobStepComplete += (JobStep) => { World.DemolishTile(Position, WorldTile.Layer); };
        if (Type == JobType.Craft) OnJobStepComplete += (JobStep) => { Inventory.Add(WorldTile.CraftYield); };
        if (Type == JobType.Harvest) OnJobStepComplete += (JobStep) => { World.HarvestTile(Position, WorldTile.Layer); };
    }

    public virtual void Work(float points)
    {
        // If the job is not already complete
        if (Cost >= 0)
        {
            // Subtract the points provided
            Cost -= points;
            // Check again and invoke if complete
            if (Cost <= 0)
            {
                Complete = true;
                OnJobStepComplete?.Invoke(this);
            }
        }
    }

    public JobStepSave Serialize()
    {
        JobStepSave save = new JobStepSave();
        // Save the required data
        save.Guid = Guid;
        save.Type = (int)Type;
        save.TileName = WorldTile.Name;
        save.PositionX = Position.x;
        save.PositionY = Position.y;
        save.Cost = Cost;
        save.Complete = Complete;
        save.Rotations = Rotations;
        // Now return the saved job step
        return save;
    }

}