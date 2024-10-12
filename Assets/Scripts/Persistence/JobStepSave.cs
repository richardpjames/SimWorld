using System;
using UnityEngine;

[Serializable]
public class JobStepSave
{
    public Guid Guid;
    public int Type;
    public string TileName;
    public int PositionX;
    public int PositionY;
    public float Cost;
    public bool Complete;
    public float RotationZ;

    public JobStep Deserialize()
    {
        PrefabFactory prefabFactory = GameObject.FindAnyObjectByType<PrefabFactory>();
        // Create a new jobstep from the recorded information
        JobStep jobStep = new JobStep((JobType)Type, prefabFactory.Create(TileName), new Vector2Int(PositionX, PositionY), Cost, Complete, Quaternion.Euler(0, 0, RotationZ));
        // Set the GUID
        jobStep.Guid = Guid;
        // Return the job step
        return jobStep;
    }
}