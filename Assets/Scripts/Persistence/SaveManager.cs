using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void Save()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        AgentPool agentPool = GameObject.FindAnyObjectByType<AgentPool>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        JobQueue jobQueue = GameObject.FindAnyObjectByType<JobQueue>();
        string saveDirectory = $"{Application.persistentDataPath}/Worlds/{world.Name}";
        string saveFile = $"{saveDirectory}/save.json";

        // Create a save and then populate it from the appropriate objects
        GameSave save = new GameSave();
        save.JobQueueSave = jobQueue.Serialize();
        save.AgentPoolSave = agentPool.Serialize();
        save.WorldSave = world.Serialize();
        save.InventorySave = inventory.Serialize();
        // Finally convert to a save file
        string jsonSave = JsonConvert.SerializeObject(save);
        // Check for the directory and create if it doesn't exist
        if(!System.IO.Directory.Exists(saveDirectory))
        {
            System.IO.Directory.CreateDirectory(saveDirectory);
        }
        // Save the file
        System.IO.File.WriteAllText(saveFile, jsonSave);
    }

    public void Load()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        AgentPool agentPool = GameObject.FindAnyObjectByType<AgentPool>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        JobQueue jobQueue = GameObject.FindAnyObjectByType<JobQueue>();

        string loadPath = $"{Application.persistentDataPath}/{world.Name}/save.json";
        // Return if the file to load doesn't exist
        if (!System.IO.File.Exists(loadPath)) return;
        // Read the data from the file
        string jsonData = System.IO.File.ReadAllText(loadPath);
        // Convert back into a game save
        GameSave save = JsonConvert.DeserializeObject<GameSave>(jsonData);
        // Then get the world to load the data
        jobQueue.Deserialize(save.JobQueueSave);
        world.Deserialize(save.WorldSave);
        inventory.Deserialize(save.InventorySave);
        agentPool.Deserialize(save.AgentPoolSave);
    }
}