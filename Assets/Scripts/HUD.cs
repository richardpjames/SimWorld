using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    [SerializeField] private World _world;

    [SerializeField] private TileInformation _tileInformationPrefab;
    [SerializeField] private CraftMenu _craftingMenuPrefab;
    private CraftMenu _craftMenu;
    private TileInformation _tileInformation;
    private Vector2Int _position;

    private void Start()
    {
        // Determine whether tile updates affect the HUD
        _world.OnTileUpdated += CheckTileUpdated;
    }

    public void HandleClick(Vector2Int position)
    {
        WorldTile structure = _world.GetWorldTile(position, WorldLayer.Structure);
        if (structure != null &&(structure.Type == TileType.CraftersTable || structure.Type == TileType.HarvestersTable))
        {
            // If a dialog is already open then we just update, otherwise create a new one
            if (_craftMenu == null)
            {
                _craftMenu = Instantiate(_craftingMenuPrefab, transform.position, Quaternion.identity);
            }
            // Refresh with the tile information
            _craftMenu.Initialize(structure);
        }
        // If a dialog is already open then we just update, otherwise create a new one
        if (_tileInformation == null)
        {
            _tileInformation = Instantiate(_tileInformationPrefab, transform.position, Quaternion.identity);
        }
        // Set the position, which forces the dialog to show the tile information
        _position = position;
        _tileInformation.SetPosition(position);
    }

    // Determine whether a change to a tile has any effect
    private void CheckTileUpdated(Vector2Int position)
    {
        // If the tile information dialog is open and looking at the updated position
        if (position == _position && _tileInformation != null)
        {
            // Setting the position again will force a refresh
            _tileInformation.SetPosition(position);
        }
    }
}
