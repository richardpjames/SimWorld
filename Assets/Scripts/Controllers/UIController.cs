using UnityEngine;
using UnityEngine.UIElements;
public class UIController : MonoBehaviour
{
    // Each of the buttons in the document
    private Button woodenWallButton;
    private Button woodenFloorButton;
    private Button rocksButton;
    private Button demolishButton;
    private Button sandButton;
    private Button grassButton;
    private Button waterButton;
    private Button saveButton;
    private Button loadButton;

    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Get each of the buttons
        woodenWallButton = root.Q<Button>("wooden-wall");
        woodenFloorButton = root.Q<Button>("wooden-floor");
        rocksButton = root.Q<Button>("rocks");
        demolishButton = root.Q<Button>("demolish");
        sandButton = root.Q<Button>("sand");
        grassButton = root.Q<Button>("grass");
        waterButton = root.Q<Button>("water");
        saveButton = root.Q<Button>("save");
        loadButton = root.Q<Button>("load");
        // Set their actions
        woodenWallButton.clicked += () => ConstructionController.Instance.SetStucture(StructureType.Wall);
        woodenFloorButton.clicked += () => ConstructionController.Instance.SetFloor(FloorType.Wooden);
        rocksButton.clicked += () => ConstructionController.Instance.SetStucture(StructureType.Rock);
        demolishButton.clicked += () => ConstructionController.Instance.SetDemolish();
        sandButton.clicked += () => ConstructionController.Instance.SetTerrain(TerrainType.Sand);
        grassButton.clicked += () => ConstructionController.Instance.SetTerrain(TerrainType.Grass);
        waterButton.clicked += () => ConstructionController.Instance.SetTerrain(TerrainType.Water);
        saveButton.clicked += () => GameController.Instance.Save();
        loadButton.clicked += () => GameController.Instance.Load();
    }

}
