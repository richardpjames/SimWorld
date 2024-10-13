using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreen;
    // Holds an instance of the game manager for us elsewhere
    public static GameManager Instance { get; private set; }

    // Holds basic information about the game
    public string WorldName = "Default";
    public int WorldWidth = 50;
    public int WorldHeight = 50;
    public int TimeMultiplier = 1;
    public Dictionary<InventoryItem, int> StartingResources;
    private GameObject _loadingScreenInstance;

    public int StartingAgents = 5;

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // If we haven't started in the main menu, then load it
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                MainMenu();
            }
        }
        else
        {
            Destroy(gameObject);
        }
        StartingResources = new Dictionary<InventoryItem, int>();
        StartingResources.Add(InventoryItem.Wood, 50);
        StartingResources.Add(InventoryItem.Stone, 50);
        StartingResources.Add(InventoryItem.Planks, 20);
        StartingResources.Add(InventoryItem.Blocks, 20);
    }

    public void StartGame()
    {
        // Set the time scale to default
        TimeMultiplier = 1;
        // Create a loading screen and don't destory on the reload of the scene
        _loadingScreenInstance = Instantiate(_loadingScreen, new Vector3(0f, 0f, 0f), Quaternion.identity);
        DontDestroyOnLoad(_loadingScreenInstance);
        SceneManager.LoadScene("World");
    }

    public void DestroyLoadingScreen()
    {
        Destroy(_loadingScreenInstance);

    }

    /// <summary>
    /// Takes the player back to the main menu and saves the game
    /// </summary>
    public void MainMenu()
    {
        // Load the main menu
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quits the application safely
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
