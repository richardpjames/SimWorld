using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void Save()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        // Create a save and then populate it from the appropriate objects
        GameSave save = new GameSave();
        save.WorldSave = world.Serialize();
        // Finally convert to a save file
        string jsonSave = JsonConvert.SerializeObject(save);
        System.IO.File.WriteAllText($"{Application.persistentDataPath}/{world.Name}.json", jsonSave);
    }

    public void Load()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        // Return if the file to load doesn't exist
        if (!System.IO.File.Exists($"{Application.persistentDataPath}/{world.Name}.json")) return;
        // Read the data from the file
        string jsonData = System.IO.File.ReadAllText($"{Application.persistentDataPath}/{world.Name}.json");
        // Convert back into a game save
        GameSave save = JsonConvert.DeserializeObject<GameSave>(jsonData);
        // Then get the world to load the data
        world.Deserialize(save.WorldSave);
    }
}