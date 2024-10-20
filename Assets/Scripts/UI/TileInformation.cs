using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileInformation : MonoBehaviour
{

    // Each of the sections for display
    [SerializeField] private GameObject _craftingSection;
    [SerializeField] private GameObject _terrainSection;
    [SerializeField] private GameObject _floorSection;
    [SerializeField] private GameObject _structureSection;
    // Each of the text areas for setting description
    [SerializeField] private TextMeshProUGUI _terrainText;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _structureText;
    // Each of the images for setting sprites
    [SerializeField] private Image _terrainImage;
    [SerializeField] private Image _floorImage;
    [SerializeField] private Image _structureImage;
    // For the crafting part of the UI
    [SerializeField] private TextMeshProUGUI _craftTableName;
    [SerializeField] private TextMeshProUGUI _jobCountText;
    [SerializeField] private Toggle _continuousToggle;

    // Hold the position
    private Vector2Int _position;
    // Get a reference to the world
    private World _world;
    // Get the job queue (for demolition)
    private JobQueue _jobQueue;
    // Get the inventory
    private Inventory _inventory;
    // Hold a reference to the crafting tile 
    private WorldTile _craftingTile;

    // Start is called before the first frame update
    void Awake()
    {
        // Find a reference to the world
        _world = GameObject.FindAnyObjectByType<World>();
        // Find a reference to the job queue
        _jobQueue = GameObject.FindAnyObjectByType<JobQueue>();
        // Find a reference to the inventory
        _inventory = GameObject.FindAnyObjectByType<Inventory>();
    }

    public void HideAll()
    {
        // Hide all of the sections of the UI
        _craftingSection.SetActive(false);
        _terrainSection.SetActive(false);
        _floorSection.SetActive(false);
        _structureSection.SetActive(false);
    }
    public void DemolishFloor()
    {
        // Check that we have a position and that the floor is not already being demolished/used
        if (_position != null && !_world.GetWorldTile(_position, WorldLayer.Floor).Reserved && _world.GetWorldTile(_position, WorldLayer.Floor).CanDemolish)
        {
            _jobQueue.Add(DemolitionJobFactory.Create(_position, WorldLayer.Floor));
        }
    }
    public void DemolishStructure()
    {
        // Check that we have a position and that the structure is not already being demolished/used
        if (_position != null && !_world.GetWorldTile(_position, WorldLayer.Structure).Reserved && _world.GetWorldTile(_position, WorldLayer.Structure).CanDemolish)
        {
            _jobQueue.Add(DemolitionJobFactory.Create(_position, WorldLayer.Structure));
        }
    }

    public void SetPosition(Vector2Int position)
    {
        _position = position;
        // Check if the position is within bounds
        if (_world.CheckBounds(position) == false)
        {
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
        // Unsubscribe from the existing tile
        if (_craftingTile != null)
        {
            _craftingTile.OnWorldTileUpdated -= RefreshCrafting;
        }
        // Decide whether to show the crafting menu
        if (structure != null && (structure.Type == TileType.CraftersTable || structure.Type == TileType.HarvestersTable))
        {
            _craftingTile = structure;
            // Initialise the toggle and title
            _continuousToggle.isOn = _craftingTile.Continuous;
            _craftTableName.text = _craftingTile.Name;
            // Print the number of current jobs
            _jobCountText.text = $"Currently Queued Jobs: {_craftingTile.JobCount}";
            // Set this area of the canvas active
            _craftingSection.SetActive(true);
            // Subscribe to new changes
            _craftingTile.OnWorldTileUpdated += RefreshCrafting;
        }
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
    }

    public void RefreshCrafting(WorldTile tile)
    {
        _jobCountText.text = $"Currently Queued Jobs: {_craftingTile.JobCount}";
    }
    public void ClearQueue()
    {
        _craftingTile.SetJobCount(0);
    }
    // The number of jobs is set on the button being clicked
    public void AddJobs(int jobs)
    {
        _craftingTile.SetJobCount(_craftingTile.JobCount + jobs);
    }
    // This updates the continuous nature based on the toggle
    public void SetContinuous()
    {
        _craftingTile.SetContinuous(_continuousToggle.isOn);
    }

}