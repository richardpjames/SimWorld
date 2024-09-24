using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class WorldController : MonoBehaviour
{
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private Sprite grassSprite;
    [SerializeField] private Sprite sandSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Wave[] waves;
    [SerializeField] private float scale;
    [SerializeField] private Vector2 offset;
    [SerializeField] public string WorldName { get; private set; } = "Default";
    private Dictionary<Vector2Int, GameObject> worldObjects;
    private Dictionary<Vector2Int, GameObject> worldStructures;
    public World World { get; private set; }

    public Grid WorldGrid { get; private set; }

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }

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
        Initialize(WorldName, new Vector2Int(width, height));
    }

    /// <summary>
    /// Creates a new game world (or resets the existing)
    /// </summary>
    public void Initialize(string name, Vector2Int size)
    {
        // Destroy objects if they exist
        if (worldObjects != null)
        {
            // Loop through each and destroy
            foreach (KeyValuePair<Vector2Int, GameObject> pair in worldObjects)
            {
                Destroy(pair.Value);
            }
        }
        // Destroy objects if they exist
        if (worldStructures != null)
        {
            // Loop through each and destroy
            foreach (KeyValuePair<Vector2Int, GameObject> pair in worldStructures)
            {
                Destroy(pair.Value);
            }
        }
        // Initialise a new world
        World = new World(name, size);
        // Initialise the world objects array
        worldObjects = new Dictionary<Vector2Int, GameObject>();
        // Initialise the structyres objects array
        worldStructures = new Dictionary<Vector2Int, GameObject>();
        // Generate a game object for each tile
        for (int x = 0; x < World.Size.x; x++)
        {
            for (int y = 0; y < World.Size.y; y++)
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
                sr.sprite = LookupSprite(World.GetTile(new Vector2Int(x, y)).Type);
                sr.sortingLayerName = "Terrain";
                // Add to the dictionary of objects
                worldObjects.Add(new Vector2Int(x, y), go);
                // Subscribe to any tile changes
                World.GetTile(new Vector2Int(x, y)).OnTileUpdated += TileUpdated;
            }
        }
        // Generate random biomes
        World.GenerateBiomes(waves, scale);
    }

    /// <summary>
    /// This should be called whenever a tile is updated at the specified position
    /// </summary>
    /// <param name="position">The world position as a Vector2</param>
    private void TileUpdated(Vector2Int position)
    {
        // Update the terrain for the tile
        GameObject go = worldObjects[position];
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = LookupSprite(World.GetTile(position).Type);
        // If a structure is removed
        if (worldStructures.ContainsKey(position) && World.GetTile(position).InstalledStructure == null)
        {
            Destroy(worldStructures[position]);
            worldStructures.Remove(position);
        }
        // If a structure is added
        if (!worldStructures.ContainsKey(position) && World.GetTile(position).InstalledStructure != null)
        {
            // Create a new game object to hold the structure sprite
            GameObject sgo = new GameObject();
            sgo.name = $"{World.GetTile(position).InstalledStructure.StructureType} at {position})";
            sgo.transform.position = new Vector3(position.x, position.y, 0);
            sgo.transform.SetParent(transform, false);
            // Hold in the dictionary of structures
            worldStructures.Add(position, sgo);
            // Add a sprite renderer with the correct sprite
            SpriteRenderer ssr = worldStructures[position].AddComponent<SpriteRenderer>();
            ssr.sortingLayerName = "Structures";
            ssr.sprite = wallSprite;
        }
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
        int worldX = (int)Mathf.Clamp(Mathf.RoundToInt(x), 0, Instance.World.Size.x - 1);
        int worldY = (int)Mathf.Clamp(Mathf.RoundToInt(y), 0, Instance.World.Size.y - 1);
        // Return the value as a vector
        return new Vector3(worldX, worldY, 0f);
    }

    /// <summary>
    /// Set the type of a tile at a sepecific world location
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="type">The type (e.g. sand)</param>
    public void SetTileType(Vector2Int position, TileType type)
    {
        // Set the tile to the new type
        World.GetTile(position).SetType(type);
    }
}
