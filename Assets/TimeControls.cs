using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControls : MonoBehaviour
{
    public void SetTimeMultiplier(int timeMultiplier)
    {
        GameManager.Instance.TimeMultiplier = timeMultiplier;
    }
}
