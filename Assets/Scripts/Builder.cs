using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Builder : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TileBase _indicatorTile;
    [SerializeField] private World _world;
    [SerializeField] private JobQueue _jobQueue;
    [SerializeField] private PrefabFactory _prefabFactory;
    [SerializeField] private Grid _grid;
    [SerializeField] private Color _baseColour;
    [SerializeField] private Color _successColour;
    [SerializeField] private Color _failureColour;
    // For updating the graphics
    private WorldTile _tile;
    private Tilemap _tilemap;
    // For keeping the mouse position at the start and end of the last frame
    private Vector3 _mousePosition = Vector3.zero;
    private Vector3 _lastMousePosition = Vector3.zero;
    // For managing dragging
    private Vector2Int _dragStart = Vector2Int.zero;
    private Vector2Int _dragEnd = Vector2Int.zero;
    private bool _dragging = false;

    public BuildMode BuildMode { get; private set; }
    // Start is called before the first frame update
    void Awake()
    {
        // Create a game object with a tilemap and a tilemap renderer
        GameObject tilemap_go = new GameObject($"Builder Tilemap");
        _tilemap = tilemap_go.AddComponent<Tilemap>();
        // Set the transparency
        _tilemap.color = _baseColour;
        // Create a renderer
        TilemapRenderer tilemapRenderer = tilemap_go.AddComponent<TilemapRenderer>();
        // Add the game object to the grid
        tilemap_go.transform.SetParent(_grid.transform);
        // Set the sorting layer for the tilemap renderer
        tilemapRenderer.sortingLayerName = $"Cursor";
        // Set the sort order to allow for Y sorting
        tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
        // Reset state 
        ClearBuildMode();
    }

    private void Update()
    {
        // Clear any temporary indicators which are shown
        _tilemap.ClearAllTiles();
        _tilemap.color = _baseColour;
        // Capture the current mouse position
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePosition.z = 0;
        // Check for UI Interaction
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Cheek for right mouse button
            if (Input.GetMouseButtonDown(1))
            {
                // Indicate that we have deselected
                ClearBuildMode();
            }
            // Rotate if allowed
            if (Input.GetKeyUp(KeyCode.Tab) && _tile != null)
            {
                _tile.Rotate();
            }
            // For structures which allow only a single tile
            if (_tile != null && _tile.BuildMode == BuildMode.Single)
            {
                SingleSelect();
            }
            // For other tile types which allow dragging or lines
            if (BuildMode == BuildMode.Demolish || (_tile != null && _tile.BuildMode == BuildMode.Drag))
            {
                DragSelect();
            }
            if (_tile != null && _tile.BuildMode == BuildMode.Line)
            {
                LineSelect();
            }
            // Show the indicator
            Indicator();
        }
        //Capture the final mouse position
        _lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lastMousePosition.z = 0;
    }

    private void Indicator()
    {
        // Set the indicator - This only happens if we are not building
        if (!_dragging && BuildMode != BuildMode.None)
        {
            // Set the indicator on the tilemap based on current position
            Vector2Int position = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            if (_tile == null)
            {
                _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), _indicatorTile);
            }
            else
            {
                _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), _tile.Tile);
                if (_tile.CheckValidity(_world, new Vector2Int(position.x, position.y)))
                {
                    _tilemap.color = _successColour;
                }
                else
                {
                    _tilemap.color = _failureColour;
                }
            }
            // If we have been provided with a world tile then apply any rotation
            if (_tile != null)
            {
                // Take the existing matrix for the tilemap
                Matrix4x4 matrix = Matrix4x4.Rotate(_tile.Rotation);
                _tilemap.SetTransformMatrix(new Vector3Int(position.x, position.y, 0), matrix);
            }
        }
    }

    private void SingleSelect()
    {
        // To avoid any issues, set dragging to false
        _dragging = false;
        // Simply detect when the mouse button is up and send this as input complete
        if (Input.GetMouseButtonUp(0))
        {
            // Convert the mouse position to world position
            Vector2Int position = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            Build(position, position);
        }
    }

    private void DragSelect()
    {
        // Get the start position for the drag
        if (Input.GetMouseButtonDown(0))
        {
            // Get an X and y position bound by the world controller
            _dragStart = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            _dragging = true;
        }
        // This is the end position for the drag
        else if (Input.GetMouseButtonUp(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Work out the top left
            Vector2Int topLeft = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), Mathf.Min(_dragStart.y, _dragEnd.y));
            // Work out the bottom right
            Vector2Int bottomRight = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), Mathf.Max(_dragStart.y, _dragEnd.y));
            // Let other components know that dragging is complete
            Build(topLeft, bottomRight);
            // Reset the dragging indicator
            _dragging = false;
        }
        // This is while we are dragging
        else if (Input.GetMouseButton(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Show the indicator over the area currently selected
            for (int x = (int)Mathf.Min(_dragStart.x, _dragEnd.x); x <= (int)Mathf.Max(_dragStart.x, _dragEnd.x); x++)
            {
                for (int y = (int)Mathf.Min(_dragStart.y, _dragEnd.y); y <= (int)Mathf.Max(_dragStart.y, _dragEnd.y); y++)
                {
                    if (_tile != null)
                    {
                        _tilemap.SetTile(new Vector3Int(x, y, 0), _tile.Tile);
                        if (_tile.CheckValidity(_world, new Vector2Int(x, y)))
                        {
                            _tilemap.SetColor(new Vector3Int(x, y, 0), _successColour);
                        }
                        else
                        {
                            _tilemap.SetColor(new Vector3Int(x, y, 0), _failureColour);
                        }
                        Matrix4x4 matrix = Matrix4x4.Rotate(_tile.Rotation);
                        _tilemap.SetTransformMatrix(new Vector3Int(x, y, 0), matrix);
                    }
                    else
                    {
                        _tilemap.SetTile(new Vector3Int(x, y, 0), _indicatorTile);
                    }
                }
            }
        }
    }

    private void LineSelect()
    {

        // Get the start position for the drag
        if (Input.GetMouseButtonDown(0))
        {
            // Get an X and y position bound by the world controller
            _dragStart = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            _dragging = true;
        }
        // This is the end position for the drag
        else if (Input.GetMouseButtonUp(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Work out the top left
            Vector2Int topLeft = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), Mathf.Min(_dragStart.y, _dragEnd.y));
            // Work out the bottom right
            Vector2Int bottomRight = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), Mathf.Max(_dragStart.y, _dragEnd.y));
            // If the X is longer
            Vector2Int start = Vector2Int.zero;
            Vector2Int end = Vector2Int.zero;
            if (bottomRight.x - topLeft.x > bottomRight.y - topLeft.y)
            {
                start = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), _dragStart.y);
                end = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), _dragStart.y);
            }
            else
            {
                start = new Vector2Int(_dragStart.x, Mathf.Min(_dragStart.y, _dragEnd.y));
                end = new Vector2Int(_dragStart.x, Mathf.Max(_dragStart.y, _dragEnd.y));
            }
            // Let other components know that dragging is complete
            Build(start, end);
            // Reset the dragging indicator
            _dragging = false;
        }
        // This is while we are dragging
        else if (Input.GetMouseButton(0))
        {
            // Get an X and y position bound by the world controller
            _dragEnd = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            // Work out the top left
            Vector2Int topLeft = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), Mathf.Min(_dragStart.y, _dragEnd.y));
            // Work out the bottom right
            Vector2Int bottomRight = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), Mathf.Max(_dragStart.y, _dragEnd.y));
            // If the X is longer
            Vector2Int start = Vector2Int.zero;
            Vector2Int end = Vector2Int.zero;
            if (bottomRight.x - topLeft.x > bottomRight.y - topLeft.y)
            {
                start = new Vector2Int(Mathf.Min(_dragStart.x, _dragEnd.x), _dragStart.y);
                end = new Vector2Int(Mathf.Max(_dragStart.x, _dragEnd.x), _dragStart.y);
            }
            else
            {
                start = new Vector2Int(_dragStart.x, Mathf.Min(_dragStart.y, _dragEnd.y));
                end = new Vector2Int(_dragStart.x, Mathf.Max(_dragStart.y, _dragEnd.y));
            }
            // Show the indicator over the area currently selected
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    if (_tile != null)
                    {
                        _tilemap.SetTile(new Vector3Int(x, y, 0), _tile.Tile);
                        if (_tile.CheckValidity(_world, new Vector2Int(x, y)))
                        {
                            _tilemap.SetColor(new Vector3Int(x, y, 0), _successColour);
                        }
                        else
                        {
                            _tilemap.SetColor(new Vector3Int(x, y, 0), _failureColour);
                        }
                        Matrix4x4 matrix = Matrix4x4.Rotate(_tile.Rotation);
                        _tilemap.SetTransformMatrix(new Vector3Int(x, y, 0), matrix);
                    }
                    else
                    {
                        _tilemap.SetTile(new Vector3Int(x, y, 0), _indicatorTile);
                    }
                }
            }
        }
    }
    private void Build(Vector2Int topLeft, Vector2Int bottomRight)
    {
        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = bottomRight.y; y >= topLeft.y; y--)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (BuildMode != BuildMode.Demolish && BuildMode != BuildMode.None)
                {
                    // Place it into the world
                    if (_tile.CheckValidity(_world, position))
                    {
                        _jobQueue.Add(Job.BuildJob(_world, position, _tile.NewInstance(), _prefabFactory));
                    }
                }
                else if (BuildMode == BuildMode.Demolish)
                {
                    // During demolision we first look for any structures (and remove) and then next, any floors
                    if (_world.GetWorldTile(position, WorldLayer.Structure) != null)
                    {
                        // If the tile is already reserved, then don't place the job
                        if (!_world.GetWorldTile(position, WorldLayer.Structure).Reserved)
                        {
                            _jobQueue.Add(Job.DemolishJob(_world, position, WorldLayer.Structure));
                        }
                    }
                    if (_world.GetWorldTile(position, WorldLayer.Floor) != null)
                    {
                        // If the tile is already reserved, then don't place the job
                        if (!_world.GetWorldTile(position, WorldLayer.Floor).Reserved)
                        {
                            _jobQueue.Add(Job.DemolishJob(_world, position, WorldLayer.Floor));
                        }
                    }
                }
            }
        }
    }

    public void ClearBuildMode()
    {
        _tile = null;
        BuildMode = BuildMode.None;
    }

    public void SetBuild(string name)
    {
        _tile = _prefabFactory.GetByName(name);
        BuildMode = _tile.BuildMode;
    }

    public void SetDemolish()
    {
        _tile = null;
        BuildMode = BuildMode.Demolish;
    }

}
