using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    private World _world;
    [SerializeField] private CraftMenu _craftingMenuPrefab;
    [SerializeField] private GameObject _tileInformationWindow;
    [SerializeField] private GameObject _inventoryWindow;
    [SerializeField] private GameObject _buildWindow;
    [SerializeField] private GameObject _pauseScreen;
    private CraftMenu _craftMenu;
    private Vector2Int _position;

    private void Start()
    {
        _world = GameObject.FindAnyObjectByType<World>();
        // Determine whether tile updates affect the HUD
        _world.OnTileUpdated += CheckTileUpdated;
    }

    private void Update()
    {
        // On pressing the escape key, show the pause screen (which will handle pausing the game)
        // or hide it again
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _pauseScreen.SetActive(!_pauseScreen.activeSelf);
        }
    }

    public void ShowPauseMenu()
    {
        _pauseScreen.SetActive(true);
    }

    // Simply show the inventory window
    public void ShowInventory()
    {
        _inventoryWindow.SetActive(true);
    }
    public void ShowBuildWindow()
    {
        _buildWindow.SetActive(true);
    }
    public void HandleClick(Vector2Int position)
    {
        // If we are on the pause screen, then simply exit
        if (_pauseScreen.activeSelf) return;
        WorldTile structure = _world.GetWorldTile(position, WorldLayer.Structure);
        if (structure != null && (structure.Type == TileType.CraftersTable || structure.Type == TileType.HarvestersTable))
        {
            // If a dialog is already open then we just update, otherwise create a new one
            if (_craftMenu == null)
            {
                _craftMenu = Instantiate(_craftingMenuPrefab, transform.position, Quaternion.identity);
            }
            // Refresh with the tile information
            _craftMenu.Initialize(structure);
        }
        // Set the position, which forces the dialog to show the tile information
        _position = position;
        _tileInformationWindow.SetActive(true);
        _tileInformationWindow.GetComponent<TileInformation>().SetPosition(position);
    }

    // Determine whether a change to a tile has any effect
    private void CheckTileUpdated(Vector2Int position)
    {
        // If the tile information dialog looking at the updated position
        if (position == _position)
        {
            // Setting the position again will force a refresh
            _tileInformationWindow.GetComponent<TileInformation>().SetPosition(position);
        }
    }
}
