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
        // Disable all UI windows by default
        _inventoryWindow.SetActive(false);
        _buildWindow.SetActive(false);
        _pauseScreen.SetActive(false);
        _tileInformationWindow.SetActive(false);
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
