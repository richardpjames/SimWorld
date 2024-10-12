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
    public Quaternion Rotation { get; protected set; }
    public Inventory Inventory { get; protected set; }

    public Action<JobStep> OnJobStepComplete;

    // FIXME: Doesn't always need the inventory, so add a check and remove unneeded references
    public JobStep(JobType type, WorldTile worldTile, Vector2Int position, float cost, bool complete, Quaternion rotation)
    {
        // Create a guid
        Guid = Guid.NewGuid();
        World = GameObject.FindAnyObjectByType<World>();
        Type = type;
        WorldTile = worldTile;
        Position = position;
        Cost = cost;
        Complete = complete;
        Rotation = rotation;
        Inventory = GameObject.FindAnyObjectByType<Inventory>();

        if (Type == JobType.Build) OnJobStepComplete += (JobStep) => { World.UpdateWorldTile(Position, WorldTile); };
        if (Type == JobType.Demolish) OnJobStepComplete += (JobStep) => { World.RemoveWorldTile(Position, WorldTile.Layer); };
        if (Type == JobType.Craft) OnJobStepComplete += (JobStep) => { Inventory.Add(WorldTile.CraftYield); };

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
        save.RotationZ = Rotation.eulerAngles.z;
        // Now return the saved job step
        return save;
    }

}