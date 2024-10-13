using System;

[Serializable]
public class AgentSave
{
    public Guid Guid;
    public Guid CurrentJobGuid;
    public float PositionX;
    public float PositionY;
    public int NextPositionX;
    public int NextPositionY;
    public float TargetX;
    public float TargetY;
    public float TargetZ;
    public AStarSave AStar;
}