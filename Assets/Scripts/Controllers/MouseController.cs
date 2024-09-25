using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour
{
    [Header("Mouse Movement")]
    [SerializeField] private float scrollSpeed = 450f;
    [SerializeField] private float zoomSpeed = 1500f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 8f;
    [Header("Cursor Display")]
    [SerializeField] private Sprite indicatorSprite;
    // For keeping the mouse position at the start and end of the last frame
    private Vector3 mousePosition = Vector3.zero;
    private Vector3 lastMousePosition = Vector3.zero;
    // For keeping track of dragging and selecting
    private Vector2Int dragStart = Vector2Int.zero;
    private Vector2Int dragEnd = Vector2Int.zero;
    private bool dragging = false;
    // For drawing the indicators
    private GameObject indicatorTilemap;
    private Tile indicatorTile;
    // To let others know when a selection is complete
    public Action<Vector2Int, Vector2Int> OnDragComplete;

    // Allow for singleton pattern
    public static MouseController Instance { get; private set; }

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

    private void Start()
    {
        // Initialise the tilemap for showing indicators
        indicatorTilemap = new GameObject("Indicators");
        // Parent to the world grid
        indicatorTilemap.transform.SetParent(WorldController.Instance.WorldGrid.transform);
        // Add the tilemap component
        indicatorTilemap.AddComponent<Tilemap>();
        TilemapRenderer indicatorRenderer = indicatorTilemap.AddComponent<TilemapRenderer>();
        indicatorRenderer.sortingLayerName = "Indicators";
        // Set up the tile
        indicatorTile = ScriptableObject.CreateInstance<Tile>();
        indicatorTile.sprite = indicatorSprite;
    }
    void Update()
    {
        // Clear any temporary indicators which are shown
        indicatorTilemap.GetComponent<Tilemap>().ClearAllTiles();
        // Capture the current mouse position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        // Check for UI Interaction
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Handle mouse scrolling
            Scroll();
            // Handle zooming
            Zoom();
            // Handle selection
            Select();
            // Show the indicator
            SetIndicator();
        }
        //Capture the final mouse position
        lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition.z = 0;
    }

    /// <summary>
    /// Deals with scrolling the map based on mouse and keyboard input
    /// </summary>
    private void Scroll()
    {
        // For panning on right or middle mouse button
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            // Get the main camera
            Camera cam = Camera.main;
            // Move it towards the mouse position based on the speed and delta time
            cam.transform.Translate((lastMousePosition - mousePosition) * (scrollSpeed / cam.orthographicSize) * Time.deltaTime);
            // Clamp the position so that the level is always visible
            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, 0, WorldController.Instance.World.Size.x), Mathf.Clamp(cam.transform.position.y, 0, WorldController.Instance.World.Size.y), cam.transform.position.z);
        }
    }

    /// <summary>
    /// Deals with zooming in and out based on mouse and keyboard input
    /// </summary>
    private void Zoom()
    {
        // Applies zoom to the camera based on zoom speed
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        // Ensure we don't zoom too far in or out
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, maxZoom, minZoom);
    }

    /// <summary>
    /// Deals with all selection, including single selection and dragging.
    /// </summary>
    private void Select()
    {
        // Get the start position for the drag
        if (Input.GetMouseButtonDown(0))
        {
            // Get an X and y position bound by the world controller
            dragStart = WorldController.Instance.GetSquarePosition(mousePosition.x, mousePosition.y);
            dragging = true;
        }
        // This is the end position for the drag
        else if (Input.GetMouseButtonUp(0))
        {
            // Get an X and y position bound by the world controller
            dragEnd = WorldController.Instance.GetSquarePosition(mousePosition.x, mousePosition.y);
            // Work out the top left
            Vector2Int topLeft = new Vector2Int(Mathf.Min(dragStart.x, dragEnd.x), Mathf.Min(dragStart.y, dragEnd.y));
            // Work out the bottom right
            Vector2Int bottomRight = new Vector2Int(Mathf.Max(dragStart.x, dragEnd.x), Mathf.Max(dragStart.y, dragEnd.y));
            // Let other components know that dragging is complete
            OnDragComplete?.Invoke(topLeft, bottomRight);
            // Reset the dragging indicator
            dragging = false;
        }
        // This is while we are dragging
        else if (Input.GetMouseButton(0))
        {
            // Get an X and y position bound by the world controller
            dragEnd = WorldController.Instance.GetSquarePosition(mousePosition.x, mousePosition.y);
            // Show the indicator over the area currently selected
            for (int x = (int)Mathf.Min(dragStart.x, dragEnd.x); x <= (int)Mathf.Max(dragStart.x, dragEnd.x); x++)
            {
                for (int y = (int)Mathf.Min(dragStart.y, dragEnd.y); y <= (int)Mathf.Max(dragStart.y, dragEnd.y); y++)
                {
                    indicatorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), indicatorTile);
                }
            }
        }
    }

    /// <summary>
    /// Move the visual indicator to the current tile which is underneath the mouse
    /// </summary>
    private void SetIndicator()
    {
        // This only happens if we are not dragging to avoid showing the indicator twice on some positions
        if (!dragging)
        {
            // Set the indicator on the tilemap based on current position
            Vector2Int position = WorldController.Instance.GetSquarePosition(mousePosition.x, mousePosition.y);
            indicatorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(position.x, position.y, 0), indicatorTile);
        }
    }
}
