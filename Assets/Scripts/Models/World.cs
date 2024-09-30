using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World
{
    // This holds our world
    private Dictionary<Vector2Int, TileType> _terrains;
    private Dictionary<Vector2Int, TileType> _structures;
    private Dictionary<Vector2Int, TileType> _floors;
    // This holds all of our jobs
    public Queue<Job> JobQueue;
    public Vector2Int Size { get; private set; }
    public string Name { get; private set; }
    // Lets others know that a tile at the specified position is updated
    public Action<Vector2Int> OnSquareUpdated;
    public Action<Job, TileType> OnJobCreated;
    public Action<Job> OnJobCompleted;

    public World(string name, Vector2Int size, TerrainDataConfiguration terrrainDataConfiguration)
    {
        // Store the name, width and height of the world
        this.Name = name;
        this.Size = size;
        // Initialise the array of tiles
        _terrains = new Dictionary<Vector2Int, TileType>();
        _structures = new Dictionary<Vector2Int, TileType>();
        _floors = new Dictionary<Vector2Int, TileType>();
        // Initialise the job queue
        JobQueue = new Queue<Job>();
        // Creates a world map with the height and width specified
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                // Default each tile to be grass in the first instance
                _terrains.Add(new Vector2Int(x, y), new Terrain(TerrainType.Grass, terrrainDataConfiguration.GetTile(TerrainType.Grass)));
            }
        }
    }

    /// <summary>
    /// Generates random biomes within the map
    /// </summary>
    public void GenerateBiomes(Wave[] waves, float scale, TerrainDataConfiguration terrainDataConfiguration)
    {
        // Generate a random offset
        Vector2 offset = new Vector2(UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500));
        // Generate a heightmap for the biomes
        float[,] heightMap = NoiseGenerator.Generate(Size, scale, waves, offset);
        // Update the tiles affected looping over the width
        for (int x = 0; x < Size.x; x++)
        {
            // Then loop over the height
            for (int y = 0; y < Size.y; y++)
            {
                // At the lowest levels we add water
                if (heightMap[x, y] < 0.2)
                {
                    Terrain terrain = Get<Terrain>(new Vector2Int(x, y));
                    terrain.SetType(TerrainType.Water, terrainDataConfiguration.GetTile(TerrainType.Water));
                }
                // Then sand as the height increases
                else if (heightMap[x, y] < 0.3)
                {
                    Terrain terrain = Get<Terrain>(new Vector2Int(x, y));
                    terrain.SetType(TerrainType.Sand, terrainDataConfiguration.GetTile(TerrainType.Sand));
                }
                // All remaining tiles are grass
                else
                {
                    Terrain terrain = Get<Terrain>(new Vector2Int(x, y));
                    terrain.SetType(TerrainType.Grass, terrainDataConfiguration.GetTile(TerrainType.Grass));
                }
            }
        }
    }

    /// <summary>
    /// Get the floor, structure or terrain at a position
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <returns></returns>
    public T Get<T>(Vector2Int position) where T : TileType
    {
        if (CheckBounds(position) && GetDictionary<T>().ContainsKey(position))
        {
            return (T)GetDictionary<T>()[position];
        }
        return null;
    }

    /// <summary>
    /// Creates a job and adds it to the queue
    /// </summary>
    /// <param name="location">The location at which the job is located</param>
    /// <param name="onJobComplete">The code to be executed on completion of the job</param>
    /// <param name="jobCost">What is the cost of the job in points</param>
    /// <returns></returns>
    public Job Install<T>(Vector2Int location, T item, float jobCost) where T : TileType, IBuildableObject
    {
        // Check if a job already exists at this location
        if (JobQueue.Any((job) => job.Location == location && job.Target == GetJobTarget<T>()) == false)
        {
            // Check if we are out of bounds or trying to build on water
            if (CheckBounds(location) && Get<Terrain>(location).TerrainType != TerrainType.Water && !GetDictionary<T>().ContainsKey(location))
            {
                Job job = new Job(location, (job) => { CompleteInstall<T>(job, item); }, jobCost, GetJobTarget<T>());
                JobQueue.Enqueue(job);
                // Make sure the right events fire
                job.OnJobComplete += OnJobCompleted;
                OnJobCreated?.Invoke(job, item);
                return job;
            }
        }
        return null;
    }
    /// <summary>
    /// Places a floor or structure into the tile
    /// </summary>
    /// <param name="floor">The floor to be installed </param>
    private void CompleteInstall<T>(Job job, T item) where T : TileType, IBuildableObject
    {
        GetDictionary<T>().Add(job.Location, item);
        OnSquareUpdated?.Invoke(job.Location);
    }

    /// <summary>
    /// Creates a job and adds it to the queue
    /// </summary>
    /// <param name="location">The location at which the job is located</param>
    /// <param name="onJobComplete">The code to be executed on completion of the job</param>
    /// <param name="jobCost">What is the cost of the job in points</param>
    /// <returns></returns>
    public Job Remove<T>(Vector2Int location, float jobCost) where T : TileType, IBuildableObject
    {
        // Check if a job already exists at this location
        if (JobQueue.Any((job) => job.Location == location && job.Target == GetJobTarget<T>()) == false)
        {
            Job job = new Job(location, (job) => { CompleteRemove<T>(job); }, jobCost, JobTarget.Demolish);
            JobQueue.Enqueue(job);
            // Set up the correct actions
            job.OnJobComplete += OnJobCompleted;
            OnJobCreated?.Invoke(job, null);
            return job;
        }
        return null;
    }

    /// <summary>
    /// Removes any floor or structure installed on this tile
    /// </summary>
    private void CompleteRemove<T>(Job job) where T : TileType, IBuildableObject
    {
        if (GetDictionary<T>().ContainsKey(job.Location))
        {
            GetDictionary<T>().Remove(job.Location);
            OnSquareUpdated?.Invoke(job.Location);
        }
    }

    /// <summary>
    /// Returns false if the vector falls outside of the world
    /// </summary>
    /// <returns></returns>
    private bool CheckBounds(Vector2Int position)
    {
        return !(position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y);
    }

    /// <summary>
    /// Gets the appropriate dictionary based on the type passed in
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private Dictionary<Vector2Int, TileType> GetDictionary<T>()
    {
        // Return the correct tilemap based on the type
        if (typeof(T) == typeof(Structure)) return _structures;
        if (typeof(T) == typeof(Floor)) return _floors;
        if (typeof(T) == typeof(Terrain)) return _terrains;
        // For all other types return null
        return null;
    }

    /// <summary>
    /// Convert types into job targets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private JobTarget GetJobTarget<T>()
    {
        if (typeof(T) == typeof(Structure)) return JobTarget.Structure;
        if (typeof(T) == typeof(Floor)) return JobTarget.Floor;
        return JobTarget.Demolish;
    }
}
