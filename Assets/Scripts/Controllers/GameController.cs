using System.IO;
using UnityEngine;
using static Save;

public class GameController : MonoBehaviour
{
    // Holds an instance of the game manager for us elsewhere
    public static GameController Instance { get; private set; }

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
                WorldController.Instance.SetSquareType(new Vector2Int(tile.X, tile.Y), tile.Type);
                // Recreate the saved structure
                if (tile.HasStructure)
                {
                    WorldController.Instance.World.GetSquare(new Vector2Int(tile.X, tile.Y)).InstallStructure(new Structure(tile.StructureType, tile.StructureMovementCost, tile.StructureWidth, tile.StructureHeight));
                }
                if (tile.HasFloor)
                {
                    WorldController.Instance.World.GetSquare(new Vector2Int(tile.X, tile.Y)).InstallFloor(new Floor(tile.FloorType, tile.FloorMovementCost));
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
        File.WriteAllText(Application.persistentDataPath + $"/SimWorld-{WorldController.Instance.WorldName}.json", data);
    }

    /// <summary>
    /// Deserialises and returns the state of the save game
    /// </summary>
    /// <returns>A save object containing the state of the game</returns>
    private Save LoadGame()
    {
        // Only load the file if it exists
        if (File.Exists(Application.persistentDataPath + $"/SimWorld-{WorldController.Instance.WorldName}.json"))
        {
            string data = File.ReadAllText(Application.persistentDataPath + $"/SimWorld-{WorldController.Instance.WorldName}.json");
            Save save = JsonUtility.FromJson<Save>(data);
            return save;
        }
        // If the file is not found then return null
        return null;
    }
}
