using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TileInformation _tileInformationPrefab;
    private TileInformation _tileInformation;
    private Vector2Int _tileInformationPosition;
    public static HUDManager Instance { get; private set; }

    private World _world { get => WorldManager.Instance.World; }

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
        // Determine whether tile updates affect the HUD
        _world.OnTileUpdated += CheckTileUpdated;
    }

    // Determine whether a change to a tile has any effect
    private void CheckTileUpdated(Vector2Int position)
    {
        // If the tile information dialog is open and looking at the updated position
        if (position == _tileInformationPosition && _tileInformation != null)
        {
            // Setting the position again will force a refresh
            _tileInformation.SetPosition(position);
        }
    }

    public void ShowTileInformation(Vector2Int position)
    {
        // If a dialog is already open then we just update, otherwise create a new one
        if (_tileInformation == null)
        {
            _tileInformation = Instantiate(_tileInformationPrefab, transform.position, Quaternion.identity);
        }
        // Set the position, which forces the dialog to show the tile information
        _tileInformationPosition = position;
        _tileInformation.SetPosition(position);
    }
}
