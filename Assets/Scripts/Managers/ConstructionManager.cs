using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TileBase indicatorTile;

    // For tracking what we are currently building
    private WorldTile _currentWorldTile;
    private BuildMode _currentBuildMode = BuildMode.None;

    // Actions
    public Action<TileBase> OnBuildingModeSet;

    // Accessors for easier access
    private World _world { get => WorldManager.Instance.World; }
    private MouseManager _mouse { get => MouseManager.Instance; }
    private JobQueue _jobQueue { get => JobManager.Instance.JobQueue; }

    // Allow for singleton pattern
    public static ConstructionManager Instance { get; private set; }

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
        _mouse.OnDragComplete += Build;
        _mouse.OnDeselectComplete += () => { _currentBuildMode = BuildMode.None; OnBuildingModeSet?.Invoke(null); };
    }


    private void Build(Vector2Int topLeft, Vector2Int bottomRight)
    {
        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = bottomRight.y; y >= topLeft.y; y--)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (_currentBuildMode == BuildMode.Build)
                {
                    // Place it into the world
                    if (_currentWorldTile.CheckValidity(_world, position))
                    {
                        _jobQueue.Add(new BuildJob(_world, position, _currentWorldTile));
                    }
                }
                else if (_currentBuildMode == BuildMode.Demolish)
                {
                    // During demolision we first look for any structures (and remove) and then next, any floors
                    if (_world.GetWorldTile(position, WorldLayer.Structure) != null)
                    {
                        _jobQueue.Add(new DemolishJob(_world, position, WorldLayer.Structure));
                    }
                    else if (_world.GetWorldTile(position, WorldLayer.Floor) != null)
                    {
                        _jobQueue.Add(new DemolishJob(_world, position, WorldLayer.Floor));
                    }
                }
            }
        }
    }

    public void SetStucture(WorldTile worldTile)
    {
        _currentBuildMode = BuildMode.Build;
        _currentWorldTile = worldTile;
        OnBuildingModeSet?.Invoke(worldTile.Tile);
    }


    public void SetDemolish()
    {
        _currentBuildMode = BuildMode.Demolish;
        OnBuildingModeSet?.Invoke(indicatorTile);
    }
}
