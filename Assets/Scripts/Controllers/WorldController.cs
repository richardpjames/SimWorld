using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private Sprite sprite;
    private GameObject[,] worldObjects;

    // Allow for a static instance and accessible variables
    public static WorldController Instance { get; private set; }
    public int Height { get => height; private set => height = value; }
    public int Width { get => width; private set => width = value; }

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
        // Initialise a new world
        World world = new World(Width, Height);
        // Initialise the world objects array
        worldObjects = new GameObject[Width, Height];
        // Generate a game object for each tile
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
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
                sr.sprite = sprite;
                sr.sortingLayerName = "Terrain";
                // Add to the array of objects
                worldObjects[x, y] = go;
            }
        }
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
}
