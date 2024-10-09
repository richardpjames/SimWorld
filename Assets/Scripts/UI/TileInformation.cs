using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileInformation : MonoBehaviour
{
    // The panel and button for tweening in and out
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _closeButton;
    // Each of the sections for display
    [SerializeField] private GameObject _terrainSection;
    [SerializeField] private GameObject _floorSection;
    [SerializeField] private GameObject _structureSection;
    // Each of the text areas for setting description
    [SerializeField] private TextMeshProUGUI _terrainText;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _structureText;
    [SerializeField] private TextMeshProUGUI _debugText;
    // Each of the images for setting sprites
    [SerializeField] private Image _terrainImage;
    [SerializeField] private Image _floorImage;
    [SerializeField] private Image _structureImage;

    // Hold the position
    private Vector2Int _position;
    // Get a reference to the world
    private World _world;
    // Get the job queue (for demolition)
    private JobQueue _jobQueue;
    // Get the inventory
    private Inventory _inventory;

    // Start is called before the first frame update
    void Awake()
    {
        // Find a reference to the world
        _world = GameObject.FindAnyObjectByType<World>();
        // Find a reference to the job queue
        _jobQueue = GameObject.FindAnyObjectByType<JobQueue>();
        // Find a reference to the inventory
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
        // Hide the panel, then scale it in
        _panel.transform.localScale = Vector3.zero;
        _closeButton.SetActive(false);
        _panel.transform.DOScale(1, 0.25f).OnComplete(() => { _closeButton.SetActive(true); });
    }

    public void Close()
    {
        // Scale out the panel and then destroy the dialog
        _closeButton.SetActive(false);
        _panel.transform.DOScale(0, 0.25f).OnComplete(() => { Destroy(gameObject); });
    }

    public void HideAll()
    {
        // Hide all of the sections of the UI
        _terrainSection.SetActive(false);
        _floorSection.SetActive(false);
        _structureSection.SetActive(false);
    }

    public void DemolishFloor()
    {
        // Check that we have a position and that the floor is not already being demolished/used
        if (_position != null && !_world.GetWorldTile(_position, WorldLayer.Floor).Reserved)
        {
            _jobQueue.Add(DemolitionJobFactory.Create(_world, _position, WorldLayer.Floor, _inventory));
        }
    }
    public void DemolishStructure()
    {
        // Check that we have a position and that the structure is not already being demolished/used
        if (_position != null && !_world.GetWorldTile(_position, WorldLayer.Structure).Reserved)
        {
            _jobQueue.Add(DemolitionJobFactory.Create(_world, _position, WorldLayer.Structure, _inventory));
        }
    }

    public void SetPosition(Vector2Int position)
    {
        _position = position;
        // Check if the position is within bounds
        if (_world.CheckBounds(position) == false)
        {
            Close();
            return;
        }
        // Set the tile and updates the UI
        WorldTile grass = _world.GetWorldTile(_position, WorldLayer.Grass);
        WorldTile sand = _world.GetWorldTile(_position, WorldLayer.Sand);
        WorldTile water = _world.GetWorldTile(_position, WorldLayer.Water);
        WorldTile floor = _world.GetWorldTile(_position, WorldLayer.Floor);
        WorldTile structure = _world.GetWorldTile(_position, WorldLayer.Structure);
        // Initially set all sections to hide until we can decide what to show
        HideAll();
        // Run through the terrain layers to decide what to show
        if (water != null)
        {
            _terrainSection.SetActive(true);
            _terrainText.text = $"{water.Name}";
            _terrainImage.sprite = _world.GetSprite(water.BasePosition, water.Layer);
        }
        else if (sand != null)
        {
            _terrainSection.SetActive(true);
            _terrainText.text = $"{sand.Name}";
            _terrainImage.sprite = _world.GetSprite(sand.BasePosition, sand.Layer);
        }
        else if (grass != null)
        {
            _terrainSection.SetActive(true);
            _terrainText.text = $"{grass.Name}";
            _terrainImage.sprite = _world.GetSprite(grass.BasePosition, grass.Layer);
        }

        // If there is a floor then display its information
        if (floor != null && floor.Type != TileType.Reserved)
        {
            _floorSection.SetActive(true);
            _floorText.text = $"{floor.Name}";
            _floorImage.sprite = _world.GetSprite(floor.BasePosition, floor.Layer);
        }
        // If there is a structure then display its information
        if (structure != null && structure.Type != TileType.Reserved)
        {
            _structureSection.SetActive(true);
            _structureText.text = $"{structure.Name}";
            _structureImage.sprite = _world.GetSprite(structure.BasePosition, structure.Layer);
        }
        // Further debug information
        _debugText.text = $"Buildable: {_world.IsBuildable(_position)}, Movement Cost: {_world.MovementCost(_position)}, Walkable: {_world.IsWalkable(_position)}, Inside: {_world.IsInside(_position)}";
    }

}
