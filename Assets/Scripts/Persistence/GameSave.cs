using System;

[Serializable]
public class GameSave
{
    public WorldSave WorldSave { get; set; }
    public AgentPoolSave AgentPoolSave { get; set; }
    public InventorySave InventorySave { get; set; }
    public JobQueueSave JobQueueSave { get; set; }
    public float CameraX { get; set; }
    public float CameraY { get; set; }
    public float CameraZ { get; set; }
    public float CameraOrthographicSize { get; set; }
}