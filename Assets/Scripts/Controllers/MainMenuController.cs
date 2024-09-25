using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Remove any unneeded managers to reset the game if coming from a world
        Destroy(GameObject.Find("WorldController"));
        Destroy(GameObject.Find("ConstructionController"));
        Destroy(GameObject.Find("MouseController"));
        // Set up actions for buttons
        root.Q<Button>("quit").clicked += () => GameController.Instance.Quit();
        root.Q<Button>("start-game").clicked += StartGame;

    }

    private void StartGame()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Set up the game controller
        GameController.Instance.WorldName = root.Q<TextField>("world-name").text;
        GameController.Instance.WorldHeight = root.Q<SliderInt>("world-height").value;
        GameController.Instance.WorldWidth = root.Q<SliderInt>("world-width").value;

        // Start the scene
        GameController.Instance.StartGame();
    }

}
