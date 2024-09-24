using Codice.CM.Client.Differences;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private Sprite grassSprite;
    [SerializeField] private Sprite sandSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;
    [SerializeField] public string WorldName { get; private set; } = "Default";
    private GameObject[,] worldObjects;
    private World world;

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }
    public int Height { get => height; private set => height = value; }
    public int Width { get => width; private set => width = value; }
    public World World { get => world; private set => world = value; }

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize(WorldName, Width, Height);
    }

    /// <summary>
    /// Creates a new game world (or resets the existing)
    /// </summary>
    public void Initialize(string name, int width, int height)
    {
        // Destroy objects if they exist
        if (worldObjects != null)
        {
            // Loop through each and destroy
            foreach (GameObject obj in worldObjects)
            {
                Destroy(obj);
            }
        }
        // Initialise a new world
        World = new World(name, width, height);
        // Initialise the world objects array
        worldObjects = new GameObject[width, height];
        // Generate a game object for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                // Create a Game Object
                GameObject go = new GameObject();
                // Set the name for the object
                go.name = $"Tile at ({x},{y})";
                // Set the parent to the manager
                go.transform.SetParent(transform, false);
                // Move this to the correct position on the grid
                go.transform.position = new Vector3(x, y, 0);
                // Add a sprite renderer
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LookupSprite(World.GetTile(x, y).Type);
                sr.sortingLayerName = "Terrain";
                // Add to the array of objects
                worldObjects[x, y] = go;
                // Subscribe to any tile changes
                World.GetTile(x, y).OnTileUpdated += TileUpdated;
            }
        }
        // Generate random biomes
        World.GenerateBiomes(waves, scale);
    }

    /// <summary>
    /// This should be called whenever a tile is updated at the specified position
    /// </summary>
    /// <param name="x">The world x position</param>
    /// <param name="y">The world y position</param>
    private void TileUpdated(int x, int y)
    {
        GameObject go = worldObjects[x, y];
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = LookupSprite(World.GetTile(x, y).Type);
    }

    private Sprite LookupSprite(TileType type)
    {
        if (type == TileType.Sand) return sandSprite;
        if (type == TileType.Water) return waterSprite;
        return grassSprite;
    }

    /// <summary>
    /// Takes a world position and determines which tile occupies that position (with an x, y coordinate returned).
    /// </summary>
    /// <param name="x">The x world position</param>
    /// <param name="y">The y world position</param>
    /// <returns>The position of the tile which occupies the requested position</returns>
    public static Vector3 GetTilePosition(float x, float y)
    {
        // Cap the world position based on the width and height
        int worldX = (int)Mathf.Clamp(Mathf.RoundToInt(x), 0, Instance.Width - 1);
        int worldY = (int)Mathf.Clamp(Mathf.RoundToInt(y), 0, Instance.Height - 1);
        // Return the value as a vector
        return new Vector3(worldX, worldY, 0f);
    }

    /// <summary>
    /// Set the type of a tile at a sepecific world location
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="type">The type (e.g. sand)</param>
    public void SetTileType(int x, int y, TileType type)
    {
        // Set the tile to the new type
        World.GetTile(x,y).SetType(type);
    }
}
