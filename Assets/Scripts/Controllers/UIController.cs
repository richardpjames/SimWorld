using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class UIController : MonoBehaviour
{
    private ConstructionController _construction { get => ConstructionController.Instance; }
    private GameController _game { get => GameController.Instance; }

    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Get each of the buttons and set their actions
        root.Q<Button>("wooden-wall").clicked += () => _construction.SetStucture(StructureType.Wall);
        root.Q<Button>("wooden-floor").clicked += () => _construction.SetFloor(FloorType.Wooden);
        root.Q<Button>("footpath").clicked += () => _construction.SetFloor(FloorType.Footpath);
        root.Q<Button>("rocks").clicked += () => _construction.SetStucture(StructureType.Rock);
        root.Q<Button>("demolish").clicked += () => _construction.SetDemolish();
        root.Q<Button>("mainmenu").clicked += () => _game.MainMenu();
    }

}
