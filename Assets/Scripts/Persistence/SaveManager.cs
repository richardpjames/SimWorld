using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private void Start()
    {
        // This script will load last (set in project settings) - so try to load the game 
        Load();
        // Then the game manager can hide its loading screen
        GameManager.Instance.DestroyLoadingScreen();
    }
    private void Update()
    {
        // Quick save on F5
        if(Input.GetKeyUp(KeyCode.F5))
        {
            Save();
        }
        // Quick load on F9
        if (Input.GetKeyUp(KeyCode.F9))
        {
            Load();
        }
    }
    public bool Save()
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
        // Store the camera position
        save.CameraX = Camera.main.transform.position.x;
        save.CameraY = Camera.main.transform.position.y;
        save.CameraZ = Camera.main.transform.position.z;
        save.CameraOrthographicSize = Camera.main.orthographicSize;
        // Finally convert to a save file
        string jsonSave = JsonConvert.SerializeObject(save);
        // Check for the directory and create if it doesn't exist
        if(!System.IO.Directory.Exists(saveDirectory))
        {
            System.IO.Directory.CreateDirectory(saveDirectory);
        }
        // Save the file
        System.IO.File.WriteAllText(saveFile, jsonSave);
        // Indicate success
        return true;
    }

    public bool Load()
    {
        // Get reference to the world
        World world = GameObject.FindAnyObjectByType<World>();
        AgentPool agentPool = GameObject.FindAnyObjectByType<AgentPool>();
        Inventory inventory = GameObject.FindAnyObjectByType<Inventory>();
        JobQueue jobQueue = GameObject.FindAnyObjectByType<JobQueue>();

        string loadPath = $"{Application.persistentDataPath}/Worlds/{world.Name}/save.json";
        // Return if the file to load doesn't exist
        if (!System.IO.File.Exists(loadPath)) return false;
        // Read the data from the file
        string jsonData = System.IO.File.ReadAllText(loadPath);
        // Convert back into a game save
        GameSave save = JsonConvert.DeserializeObject<GameSave>(jsonData);
        // Then get the world to load the data
        jobQueue.Deserialize(save.JobQueueSave);
        world.Deserialize(save.WorldSave);
        inventory.Deserialize(save.InventorySave);
        agentPool.Deserialize(save.AgentPoolSave);
        // Set the camera
        Camera.main.transform.position = new Vector3(save.CameraX, save.CameraY, save.CameraZ);
        Camera.main.orthographicSize = save.CameraOrthographicSize;
        // Indicate success
        return true;
    }
}