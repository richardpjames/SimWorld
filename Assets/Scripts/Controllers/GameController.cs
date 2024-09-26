using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Save;

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
        // Finally, try and load if possible
        Load();
    }

    /// <summary>
    /// Saves the current world to a file
    /// </summary>
    public void Save()
    {
        Save save = new Save();
        save.PopulateFromWorld(WorldController.Instance.World);
        SaveGame(save);
    }

    /// <summary>
    /// Loads the current world from a file
    /// </summary>
    public void Load()
    {
        Save save = LoadGame();
        if (save != null)
        {
            // Set up a new world
            WorldController.Instance.Initialize(save.WorldName, new Vector2Int(save.WorldWidth, save.WorldHeight));
            // Populate all of the tiles correctly
            foreach (SaveTile tile in save.Tiles)
            {
                WorldController.Instance.SetTerrainType(new Vector2Int(tile.X, tile.Y), tile.Type);
                // Recreate the saved structure
                if (tile.HasStructure)
                {
                    WorldController.Instance.InstallStructure(new Vector2Int(tile.X, tile.Y),new Structure(tile.StructureType, tile.StructureMovementCost, tile.StructureWidth, tile.StructureHeight));
                }
                if (tile.HasFloor)
                {
                    WorldController.Instance.InstallFloor(new Vector2Int(tile.X, tile.Y), new Floor(tile.FloorType, tile.FloorMovementCost));
                }
            }
        }
    }

    /// <summary>
    /// Serialises and saves the provided save object
    /// </summary>
    /// <param name="save">A save object containing the data to be persisted</param>
    private void SaveGame(Save save)
    {
        string data = JsonUtility.ToJson(save);
        File.WriteAllText(Application.persistentDataPath + $"/SimWorld-{GameController.Instance.WorldName}.json", data);
    }

    /// <summary>
    /// Deserialises and returns the state of the save game
    /// </summary>
    /// <returns>A save object containing the state of the game</returns>
    private Save LoadGame()
    {
        // Only load the file if it exists
        if (File.Exists(Application.persistentDataPath + $"/SimWorld-{GameController.Instance.WorldName}.json"))
        {
            string data = File.ReadAllText(Application.persistentDataPath + $"/SimWorld-{GameController.Instance.WorldName}.json");
            Save save = JsonUtility.FromJson<Save>(data);
            return save;
        }
        // If the file is not found then return null
        return null;
    }

    /// <summary>
    /// Takes the player back to the main menu and saves the game
    /// </summary>
    public void MainMenu()
    {
        Save();
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
