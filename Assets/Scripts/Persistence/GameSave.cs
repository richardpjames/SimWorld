using System;

[Serializable]
public class GameSave
{
    public WorldSave WorldSave { get; set; }
    public AgentPoolSave AgentPoolSave { get; set; }
    public InventorySave InventorySave { get; set; }
    public JobQueueSave JobQueueSave { get; set; }
}