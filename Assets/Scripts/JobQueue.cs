using System;
using System.Collections.Generic;
using System.Linq;
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
    private Queue<Guid> _queue;
    private Dictionary<Guid, Job> _jobRegister;

    private void Awake()
    {
        // Initialise the general queue and the register
        _queue = new Queue<Guid>();
        _jobRegister = new Dictionary<Guid, Job>();
    }

    private void Start()
    {
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        _world = GameObject.FindAnyObjectByType<World>();
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
        if (_queue.Any<Guid>((queuedJobGuid) => GetJob(queuedJobGuid).CurrentJobStep.Position == job.CurrentJobStep.Position &&
        GetJob(queuedJobGuid).CurrentJobStep.WorldTile.Layer == job.CurrentJobStep.WorldTile.Layer))
        {
            return false;
        }
        // Register the job 
        RegisterJob(job);
        _queue.Enqueue(job.Guid);
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

    public Job GetNext(Agent agent)
    {
        if (_queue.Count > 0)
        {
            Job selectedJob = GetJob(_queue.Dequeue());
            // Assign to the appropriate agent
            selectedJob.AssignedAgent = agent.Guid;
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
                    _queue.Enqueue(selectedJob.Guid);
                    return null;
                }

            }
            return selectedJob;
        }
        return null;
    }

    // Adds a job to the central register
    public void RegisterJob(Job job)
    {
        // Avoid duplicates by checking the key
        if (!_jobRegister.ContainsKey(job.Guid))
        {
            // Add the job to the register
            _jobRegister.Add(job.Guid, job);
            // Unregister the job once it is complete
            job.OnJobComplete += (job) => UnregisterJob(job.Guid);
        }
    }

    // Gets a job from the central register
    public Job GetJob(Guid jobGuid)
    {
        // May return null if this does not exist
        if (!_jobRegister.ContainsKey(jobGuid)) return null;
        return _jobRegister[jobGuid];
    }

    // Removes a job from the central register
    public void UnregisterJob(Guid jobGuid)
    {
        // Check that the key exists and then remove
        if (_jobRegister.ContainsKey(jobGuid))
        {
            _jobRegister.Remove(jobGuid);
        }
    }

    public JobQueueSave Serialize()
    {
        JobQueueSave save = new JobQueueSave();
        // Create a list for our job saves
        List<JobSave> jobs = new List<JobSave>();
        // Loop through the entire register and serialize
        foreach (Job job in _jobRegister.Values)
        {
            jobs.Add(job.Serialize());
        }
        // Convert to the array
        save.Register = jobs.ToArray();
        // Convert the job queue to an array
        save.JobQueue = _queue.ToArray();
        // Return the save
        return save;
    }

    public void Deserialize(JobQueueSave save)
    {
        foreach (WorldLayer layer in Enum.GetValues(typeof(WorldLayer)))
        {
            _tilemaps[layer].ClearAllTiles();
        }
        // Reset the queue and register
        _queue = new Queue<Guid>();
        _jobRegister = new Dictionary<Guid, Job>();
        // Load each of the jobs into the register
        foreach (JobSave jobSave in save.Register)
        {
            RegisterJob(jobSave.Deserialize());
        }
        // Queue all of those in the list
        foreach (Guid jobGuid in save.JobQueue)
        {
            Add(GetJob(jobGuid));
        }
    }
}