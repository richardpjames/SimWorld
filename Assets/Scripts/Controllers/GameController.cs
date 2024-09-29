using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Holds an instance of the game manager for us elsewhere
    public static GameController Instance { get; private set; }

    // Holds basic information about the game
    public string WorldName = "Default";
    public int WorldWidth = 50;
    public int WorldHeight = 50;

    private void Awake()
    {
        // Ensure that this is the only instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("World");
        StartCoroutine(Initilize());
    }

    private IEnumerator Initilize()
    {
        // Wait for the world controller to be created
        while (WorldController.Instance == null)
        {
            yield return null;
        }
        // Then initialize the world
        WorldController.Instance.Initialize(WorldName, new Vector2Int(WorldWidth, WorldHeight));
    }

    /// <summary>
    /// Takes the player back to the main menu and saves the game
    /// </summary>
    public void MainMenu()
    {
        // Remove any unneeded managers to reset the game
        Destroy(GameObject.Find("WorldController"));
        Destroy(GameObject.Find("ConstructionController"));
        Destroy(GameObject.Find("MouseController"));
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
