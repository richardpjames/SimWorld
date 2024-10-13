using PlasticGui.WorkspaceWindow.Locks;
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
    private Dictionary<Guid, Job> _jobRegister;

    private void Awake()
    {
        // Initialise the general register
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
        if (_jobRegister.Values.Any<Job>((queuedJob) => queuedJob.CurrentJobStep.Position == job.CurrentJobStep.Position &&
        queuedJob.CurrentJobStep.WorldTile.Layer == job.CurrentJobStep.WorldTile.Layer))
        {
            return false;
        }
        // Avoid duplicates by checking the key
        if (!_jobRegister.ContainsKey(job.Guid))
        {
            // Add the job to the register
            _jobRegister.Add(job.Guid, job);
            // Record the time that the job was added to the queue
            job.TimeQueued = Time.time;
            // Unregister the job once it is complete
            job.OnJobComplete += (job) => UnregisterJob(job.Guid);
        }
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
        if (_jobRegister.Count > 0)
        {
            float minTime = float.MaxValue;
            Job selectedJob = null;
            foreach (Job candidate in _jobRegister.Values)
            {
                // We don't want any where the job is complete or assigned
                if (candidate.Complete || candidate.AssignedAgent != Guid.Empty) continue;
                // We don't want any where we cannot afford the cost
                if (candidate.Cost != null && !_inventory.Check(candidate.Cost)) continue;
                // Check the time it was added and select if lowest (this means we return the oldest suitable job)
                if (candidate.TimeQueued < minTime)
                {
                    selectedJob = candidate;
                    minTime = candidate.TimeQueued;
                }
            }
            // If that process has been able to select a job
            if (selectedJob != null)
            {
                // Assign the selected job and return it
                selectedJob.AssignedAgent = agent.Guid;
                return selectedJob;
            }

        }
        // If there are no jobs or we couldn't find a suitable one then return null
        return null;
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
        _jobRegister = new Dictionary<Guid, Job>();
        // Load each of the jobs into the register
        foreach (JobSave jobSave in save.Register)
        {
            Add(jobSave.Deserialize());
        }
    }
}