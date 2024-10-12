using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void Save()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        string savePath = $"{Application.persistentDataPath}/{world.Name}.json";
        // Create a save and then populate it from the appropriate objects
        GameSave save = new GameSave();
        save.WorldSave = world.Serialize();
        // Finally convert to a save file
        string jsonSave = JsonConvert.SerializeObject(save);
        System.IO.File.WriteAllText(savePath, jsonSave);
    }

    public void Load()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        string loadPath = $"{Application.persistentDataPath}/{world.Name}.json";
        // Return if the file to load doesn't exist
        if (!System.IO.File.Exists(loadPath)) return;
        // Read the data from the file
        string jsonData = System.IO.File.ReadAllText(loadPath);
        // Convert back into a game save
        GameSave save = JsonConvert.DeserializeObject<GameSave>(jsonData);
        // Then get the world to load the data
        world.Deserialize(save.WorldSave);
    }
}