using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MouseManager : MonoBehaviour
{
    [Header("Mouse Movement")]
    [SerializeField] private float scrollSpeed = 450f;
    [SerializeField] private float zoomSpeed = 1500f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 8f;
    [Header("Cursor Display")]
    [SerializeField] private Tilemap indicatorTilemap;

    // For keeping the mouse position at the start and end of the last frame
    private Vector3 _mousePosition = Vector3.zero;
    private Vector3 _lastMousePosition = Vector3.zero;
    // For keeping track of dragging and selecting
    private TileBase _indicator;
    private WorldTile _tile;
    private Vector2Int _dragStart = Vector2Int.zero;
    private Vector2Int _dragEnd = Vector2Int.zero;
    private bool _dragging = false;
    // To let others know when a selection is complete
    public Action<Vector2Int, Vector2Int> OnDragComplete;
    public Action OnDeselectComplete;
    public Action OnRotationComplete;

    // Accessors for easier access to controllers etc.
    private World _world { get => WorldManager.Instance.World; }
    private GraphicsManager _graphics { get => GraphicsManager.Instance; }
    private ConstructionManager _construction { get => ConstructionManager.Instance; }

    // Allow for singleton pattern
    public static MouseManager Instance { get; private set; }

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
        // Subscribe to the construction controller action
        _construction.OnBuildingModeSet += (TileBase tile, WorldTile worldTile) => { _indicator = tile; _tile = worldTile; };
    }

    void Update()
    {
        // Clear any temporary indicators which are shown
        indicatorTilemap.GetComponent<Tilemap>().ClearAllTiles();
        // Capture the current mouse position
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePosition.z = 0;
        // Check for UI Interaction
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Handle mouse scrolling
            Scroll();
            // Handle zooming
            Zoom();
            // Handle selection
            Select();
            // Handle deselection
            Deselect();
            // Handle rotation
            Rotate();
            // Show the indicator
            SetIndicator();
        }
        //Capture the final mouse position
        _lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lastMousePosition.z = 0;
    }

    private void Rotate()
    {
        // If the tab key is pressed then simply signal a rotation
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            OnRotationComplete?.Invoke();
        }
    }

    /// <summary>
    /// Deals with scrolling the map based on mouse and keyboard input
    /// </summary>
    private void Scroll()
    {
        // For panning on middle mouse button
        if (Input.GetMouseButton(2))
        {
            // Get the main camera
            Camera cam = Camera.main;
            // Move it towards the mouse position based on the speed and delta time
            cam.transform.Translate((_lastMousePosition - _mousePosition) * (scrollSpeed / cam.orthographicSize) * Time.deltaTime);
            // Clamp the position so that the level is always visible
            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, 0, _world.Size.x), Mathf.Clamp(cam.transform.position.y, 0, _world.Size.y), cam.transform.position.z);
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
            _dragStart = _graphics.GetTilePosition(_mousePosition.x, _mousePosition.y);
            _dragging = true;
        }
        // This is the end position for the drag
        else if (Input.GetMouseButtonUp(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _graphics.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Work out the top left
            Vector2Int topLeft = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), Mathf.Min(_dragStart.y, _dragEnd.y));
            // Work out the bottom right
            Vector2Int bottomRight = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), Mathf.Max(_dragStart.y, _dragEnd.y));
            // Let other components know that dragging is complete
            OnDragComplete?.Invoke(topLeft, bottomRight);
            // Reset the dragging indicator
            _dragging = false;
        }
        // This is while we are dragging
        else if (Input.GetMouseButton(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _graphics.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Show the indicator over the area currently selected
            for (int x = (int)Mathf.Min(_dragStart.x, _dragEnd.x); x <= (int)Mathf.Max(_dragStart.x, _dragEnd.x); x++)
            {
                for (int y = (int)Mathf.Min(_dragStart.y, _dragEnd.y); y <= (int)Mathf.Max(_dragStart.y, _dragEnd.y); y++)
                {
                    indicatorTilemap.SetTile(new Vector3Int(x, y, 0), _indicator);
                    // Take the existing matrix for the tilemap
                    if (_tile != null)
                    {
                        Matrix4x4 matrix = Matrix4x4.Rotate(_tile.Rotation);
                        indicatorTilemap.SetTransformMatrix(new Vector3Int(x, y, 0), matrix);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Allows the user to deselect using the right mouse button
    /// </summary>
    private void Deselect()
    {
        // Chcek for right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            // Indicate that we have deselected
            OnDeselectComplete?.Invoke();
        }
    }

    /// <summary>
    /// Move the visual indicator to the current tile which is underneath the mouse
    /// </summary>
    private void SetIndicator()
    {
        // This only happens if we are not dragging to avoid showing the indicator twice on some positions
        if (!_dragging)
        {
            // Set the indicator on the tilemap based on current position
            Vector2Int position = _graphics.GetTilePosition(_mousePosition.x, _mousePosition.y);
            indicatorTilemap.SetTile(new Vector3Int(position.x, position.y, 0), _indicator);
            // If we have been provided with a world tile then apply any rotation
            if (_tile != null)
            {
                // Take the existing matrix for the tilemap
                Matrix4x4 matrix = Matrix4x4.Rotate(_tile.Rotation);
                indicatorTilemap.SetTransformMatrix(new Vector3Int(position.x, position.y, 0), matrix);
            }
        }
    }
}
