using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 450f;
    [SerializeField] private float zoomSpeed = 1500f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 8f;
    [SerializeField] private GameObject indicator;
    [SerializeField] private GameObject indicatorPrefab;
    // Keep track of spawned indicators so we can remove them when not needed
    private List<GameObject> spawnedIndicators = new List<GameObject>();
    // For keeping the mouse position at the start and end of the last frame
    private Vector3 mousePosition = Vector3.zero;
    private Vector3 lastMousePosition = Vector3.zero;
    // For keeping track of dragging and selecting
    private Vector3 dragStart = Vector3.zero;
    private Vector3 dragEnd = Vector3.zero;

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

    void Update()
    {
        //Capture the current mouse position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        // Handle mouse scrolling
        Scroll();
        // Handle zooming
        Zoom();
        // Handle selection
        Select();
        // Show the indicator
        SetIndicator();
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
            Camera.main.transform.Translate((lastMousePosition - mousePosition) * scrollSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Deals with zooming in and out based on mouse and keyboard input
    /// </summary>
    private void Zoom()
    {
        // Applies zoom to the camera based on zoom speed
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
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
            dragStart = WorldController.GetTilePosition(mousePosition.x, mousePosition.y);
        }
        // This is the end position for the drag
        else if (Input.GetMouseButtonUp(0))
        {
            // Clear any temporary indicators which are shown
            ClearIndicators();
            // Get an X and y position bound by the world controller
            dragEnd = WorldController.GetTilePosition(mousePosition.x, mousePosition.y);
            // For now set the type for the tile
            for (int x = (int)Mathf.Min(dragStart.x, dragEnd.x); x <= (int)Mathf.Max(dragStart.x, dragEnd.x); x++)
            {
                for (int y = (int)Mathf.Min(dragStart.y, dragEnd.y); y <= (int)Mathf.Max(dragStart.y, dragEnd.y); y++)
                {
                    WorldController.Instance.SetTileType(x, y, TileType.Sand);
                }
            }
            // Display the default indicator again
            indicator.SetActive(true);
        }
        // This is while we are dragging
        else if (Input.GetMouseButton(0))
        {
            // Clear any temporary indicators which are shown
            ClearIndicators();
            // Hide the default indicator while we are dragging
            indicator.SetActive(false);
            // Get an X and y position bound by the world controller
            dragEnd = WorldController.GetTilePosition(mousePosition.x, mousePosition.y);
            // Show the indicator over the area currently selected
            for (int x = (int)Mathf.Min(dragStart.x, dragEnd.x); x <= (int)Mathf.Max(dragStart.x, dragEnd.x); x++)
            {
                for (int y = (int)Mathf.Min(dragStart.y, dragEnd.y); y <= (int)Mathf.Max(dragStart.y, dragEnd.y); y++)
                {
                    // Spawn the indicator and keep track in the list (which is cleared each frame)
                    GameObject indicator = SimplePool.Spawn(indicatorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    indicator.transform.SetParent(transform, false);
                    spawnedIndicators.Add(indicator);
                }
            }
        }
    }

    /// <summary>
    /// Clear any indicators which are shown during the process of dragging
    /// </summary>
    private void ClearIndicators()
    {
        // Object pooling for any already spawned indicators
        while (spawnedIndicators.Count > 0)
        {
            // Remove all spawned objects and release to the pool
            GameObject go = spawnedIndicators[0];
            spawnedIndicators.RemoveAt(0);
            SimplePool.Despawn(go);
        }
    }

    /// <summary>
    /// Move the visual indicator to the current tile which is underneath the mouse
    /// </summary>
    private void SetIndicator()
    {
        indicator.transform.position = WorldController.GetTilePosition(mousePosition.x, mousePosition.y);
    }
}
