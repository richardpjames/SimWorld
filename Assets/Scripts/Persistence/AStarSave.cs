using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AStarSave
{
    public AStarPoint[] Points;

    public AStar Deserialize()
    {
        AStar aStar = new AStar();
        Queue<Vector2Int> newPath = new Queue<Vector2Int>();
        // Loop backwards through the array to recreate the queue
        for(int x = Points.Length -1; x >= 0; x--)
        {
            newPath.Enqueue(new Vector2Int(Points[x].x, Points[x].y));
        }
        aStar.Path = newPath;
        return aStar;
    }
}

[Serializable]
public struct AStarPoint
{
    public int x; 
    public int y;
}