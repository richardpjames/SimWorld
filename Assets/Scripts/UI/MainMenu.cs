using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private GameManager _game { get => GameManager.Instance; }
    // Start is called before the first frame update
    void Start()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Set up actions for buttons
        root.Q<Button>("quit").clicked += () => _game.Quit();
        root.Q<Button>("start-game").clicked += StartGame;

    }

    private void StartGame()
    {
        // Get the root from the document
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // Set up the game controller
        _game.WorldName = root.Q<TextField>("world-name").text;
        _game.WorldHeight = root.Q<SliderInt>("world-height").value;
        _game.WorldWidth = root.Q<SliderInt>("world-width").value;

        // Start the scene
        _game.StartGame();
    }

}
