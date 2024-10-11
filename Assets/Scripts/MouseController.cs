using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    private World _world;

    [Header("Mouse Movement")]
    [SerializeField] private float scrollSpeed = 450f;
    [SerializeField] private float zoomSpeed = 1500f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 8f;
    [Header("Cursor Display")]
    private HUD _hud;
    private Builder _builder;
    // For keeping the mouse position at the start and end of the last frame
    private Vector3 _mousePosition = Vector3.zero;
    private Vector3 _lastMousePosition = Vector3.zero;
    private bool _dragging = false;
    // Accessors for easier access to controllers etc.

    void Update()
    {
        _builder = GameObject.FindAnyObjectByType<Builder>();
        _world = GameObject.FindAnyObjectByType<World>();
        _hud = GameObject.FindAnyObjectByType<HUD>();
        // Capture the current mouse position
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePosition.z = 0;
        // Check for UI Interaction
        if (!EventSystem.current.IsPointerOverGameObject() || _dragging)
        {
            // Handle mouse scrolling
            Scroll();
            // Handle zooming
            Zoom();
            if (_builder.BuildMode == BuildMode.None)
            {
                Query();
            }
        }
        //Capture the final mouse position
        _lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lastMousePosition.z = 0;
    }

    private void Scroll()
    {
        // Reset dragging
        _dragging = false;
        // For panning on middle mouse button
        if (Input.GetMouseButton(2))
        {
            // If the mouse button is down then we are dragging
            _dragging = true;
            // Get the main camera
            Camera cam = Camera.main;
            // Move it towards the mouse position based on the speed and delta time
            cam.transform.Translate((_lastMousePosition - _mousePosition) * (scrollSpeed / cam.orthographicSize) * Time.deltaTime);
            // Clamp the position so that the level is always visible
            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, 0, _world.Size.x), Mathf.Clamp(cam.transform.position.y, 0, _world.Size.y), cam.transform.position.z);
        }
    }

    private void Zoom()
    {
        // Applies zoom to the camera based on zoom speed
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        // Ensure we don't zoom too far in or out
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, maxZoom, minZoom);
    }

    private void Query()
    {
        // Simply detect when the mouse button is up and send this as input complete
        if (Input.GetMouseButtonUp(0))
        {
            // Convert the mouse position to world position
            Vector2Int position = _world.GetTilePosition(_mousePosition.x, _mousePosition.y);
            _hud.HandleClick(position);
        }
    }
}
