using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class UIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Get each of the buttons and set their actions
        root.Q<Button>("wooden-wall").clicked += () => ConstructionController.Instance.SetStucture(StructureType.Wall);
        root.Q<Button>("wooden-floor").clicked += () => ConstructionController.Instance.SetFloor(FloorType.Wooden);
        root.Q<Button>("footpath").clicked += () => ConstructionController.Instance.SetFloor(FloorType.Footpath);
        root.Q<Button>("rocks").clicked += () => ConstructionController.Instance.SetStucture(StructureType.Rock);
        root.Q<Button>("demolish").clicked += () => ConstructionController.Instance.SetDemolish();
        root.Q<Button>("mainmenu").clicked += () => GameController.Instance.MainMenu();
    }

}
