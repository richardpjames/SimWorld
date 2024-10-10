using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobQueue : MonoBehaviour
{
    private World _world;
    private Inventory _inventory;
    [SerializeField] private Grid _grid;
    [SerializeField] private TileBase _demolitionTile;
    [SerializeField] private Color _baseColour;
    private Dictionary<WorldLayer, Tilemap> _tilemaps;

    // The queue of jobs
    private Queue<Job> _queue;

    private void Start()
    {
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _world = GameObject.FindAnyObjectByType<World>();
        _queue = new Queue<Job>();
        // Initialise the list of tilemaps
        _tilemaps = new Dictionary<WorldLayer, Tilemap>();
        // Generate all required tilemaps
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            // Create a game object with a tilemap and a tilemap renderer
            GameObject tilemap_go = new GameObject($"Job tilemap for {layer.ToString()}");
            Tilemap tilemap = tilemap_go.AddComponent<Tilemap>();
            // Set the transparency
            tilemap.color = _baseColour;
            // Create a renderer
            TilemapRenderer tilemapRenderer = tilemap_go.AddComponent<TilemapRenderer>();
            // Add the game object to the grid
            tilemap_go.transform.SetParent(_grid.transform);
            // Set the sorting layer for the tilemap renderer
            tilemapRenderer.sortingLayerName = $"{layer.ToString()} Jobs";
            // Set the sort order to allow for Y sorting
            tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            // Hold a reference to the tilemap in the dictionary
            _tilemaps.Add(layer, tilemap);
        }
    }

    // Add a job to the queue
    public bool Add(Job job)
    {
        // Check if any jobs of the same type already exist at this position -- FIXME: This doesn't work for multi step jobs
        if (_queue.Any<Job>((queuedJob) => queuedJob.CurrentJobStep.Position == job.CurrentJobStep.Position &&
        queuedJob.CurrentJobStep.WorldTile.Layer == job.CurrentJobStep.WorldTile.Layer))
        {
            return false;
        }
        // If not then add to the queue
        _queue.Enqueue(job);
        // Hook up the actions
        job.OnJobStepComplete += OnJobStepComplete;
        job.OnNextJobStep += OnNextJobStep;
        // Update for the first job step
        OnNextJobStep(job.CurrentJobStep);
        // Return true to notify complete
        return true;
    }

    // Clear up the tilemap after a job is complete
    private void OnJobStepComplete(JobStep jobStep)
    {
        Tilemap tilemap;
        // Find the correct tilemap for the job
        if (jobStep.Type == JobType.Demolish || jobStep.Type == JobType.Harvest) tilemap = _tilemaps[WorldLayer.Demolition];
        else tilemap = _tilemaps[jobStep.WorldTile.Layer];
        // Remove the tile from the map
        tilemap.SetTile(new Vector3Int(jobStep.Position.x, jobStep.Position.y, 0), null);
    }

    private void OnNextJobStep(JobStep jobStep)
    {
        // Update the tilemaps
        Tilemap tilemap = null;
        // If there is a tilemap and tile then update 
        if (jobStep.Type == JobType.Demolish || jobStep.Type == JobType.Harvest)
        {
            tilemap = _tilemaps[WorldLayer.Demolition];
            tilemap.SetTile(new Vector3Int(jobStep.Position.x, jobStep.Position.y, 0), _demolitionTile);
        }
        //else check if there is an indicator provided on the job
        else if (jobStep.Indicator != null)
        {
            // Set the tilemap tile accordingly
            _tilemaps[jobStep.WorldTile.Layer].SetTile(new Vector3Int(jobStep.Position.x, jobStep.Position.y, 0), jobStep.Indicator);
            Matrix4x4 matrix = Matrix4x4.Rotate(jobStep.Rotation);
            _tilemaps[jobStep.WorldTile.Layer].SetTransformMatrix(new Vector3Int(jobStep.Position.x, jobStep.Position.y, 0), matrix);
        }
    }

    public Job GetNext()
    {
        if (_queue.Count > 0)
        {
            Job selectedJob = _queue.Dequeue();
            // If this is a building job, then ensure we can afford it
            if (selectedJob.Cost != null)
            {
                // If we can afford it, then take the resources from the inventory now
                if (_inventory.Check(selectedJob.Cost))
                {
                    _inventory.Spend(selectedJob.Cost);
                    return selectedJob;
                }
                // Otherwise put back on the bottom of the queue and return null (agent to check again on next frame)
                else
                {
                    _queue.Enqueue(selectedJob);
                    return null;
                }

            }
            return selectedJob;
        }
        return null;
    }

}