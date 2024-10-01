using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Holds an instance of the game manager for us elsewhere
    public static GameManager Instance { get; private set; }

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
    }

    public void StartGame()
    {
        SceneManager.LoadScene("World");
    }

    /// <summary>
    /// Takes the player back to the main menu and saves the game
    /// </summary>
    public void MainMenu()
    {
        // Remove any unneeded managers to reset the game
        Destroy(GameObject.Find("World Manager"));
        Destroy(GameObject.Find("Construction Manager"));
        Destroy(GameObject.Find("Graphics Manager"));
        Destroy(GameObject.Find("Mouse Manager"));
        Destroy(GameObject.Find("Agent Manager"));
        Destroy(GameObject.Find("Job Manager"));
        Destroy(GameObject.Find("Prefab Manager"));

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
