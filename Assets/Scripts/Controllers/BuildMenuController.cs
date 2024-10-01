using UnityEngine;
using UnityEngine.UIElements;
public class BuildMenuController : MonoBehaviour
{
    private ConstructionController _construction { get => ConstructionController.Instance; }
    private GameController _game { get => GameController.Instance; }

    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Get each of the buttons and set their actions - with the main menu first
        root.Q<Button>("walls-menu-button").clicked += () => { ShowMenu(root, "walls-menu"); };
        root.Q<Button>("floors-menu-button").clicked += () => { ShowMenu(root, "floors-menu"); };
        root.Q<Button>("tools-menu-button").clicked += () => { ShowMenu(root, "tools-menu"); };
        root.Q<Button>("quit-button").clicked += () => { _game.MainMenu(); };
        // Then each of the construction buttons
        root.Q<Button>("wooden-walls-button").clicked += () => _construction.SetStucture(StructureType.Wall);
        root.Q<Button>("wooden-floor-button").clicked += () => _construction.SetFloor(FloorType.Wooden);
        root.Q<Button>("footpath-button").clicked += () => _construction.SetFloor(FloorType.Footpath);
        root.Q<Button>("demolish-button").clicked += () => _construction.SetDemolish();
        // Hide the sub menus by default
        HideMenus(root);
    }

    private void ShowMenu(VisualElement root, string name)
    {
        // Hide all of the menus
        HideMenus(root);
        // Then show the one requested
        root.Q<VisualElement>(name).style.display = DisplayStyle.Flex;
    }

    private void HideMenus(VisualElement root)
    {
        root.Q<VisualElement>("walls-menu").style.display = DisplayStyle.None;
        root.Q<VisualElement>("floors-menu").style.display = DisplayStyle.None;
        root.Q<VisualElement>("tools-menu").style.display = DisplayStyle.None;
    }

}
