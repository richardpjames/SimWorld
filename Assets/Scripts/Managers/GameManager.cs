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
    public Dictionary<InventoryItem, int> StartingResources;

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
        StartingResources.Add(InventoryItem.Wood, 20);
        StartingResources.Add(InventoryItem.Stone, 20);
        StartingResources.Add(InventoryItem.Planks, 20);
        StartingResources.Add(InventoryItem.Blocks, 20);
    }

    public void StartGame()
    {
        // Create a loading screen and don't destory on the reload of the scene
        GameObject loadingScreen = Instantiate(_loadingScreen, new Vector3(0f, 0f, 0f), Quaternion.identity);
        DontDestroyOnLoad(loadingScreen);
        SceneManager.LoadScene("World");
        // After the scene has loaded, destroy the loading screen
        StartCoroutine(CheckStart(loadingScreen));
    }

    private IEnumerator CheckStart(GameObject loadingScreen)
    {
        // Check for instances of the world manager being complete and then subscribe
        while(GameObject.FindAnyObjectByType<World>() == null)
        {
            yield return null;
        }
        Destroy(loadingScreen);
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
