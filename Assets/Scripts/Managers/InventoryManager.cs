using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // The players inventory
    private Inventory Inventory;
    public Dictionary<InventoryItem,int> Items {get => Inventory.Items; }

    public static InventoryManager Instance { get; private set; }

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
        // Create a new pool with the initial number of agents
        Inventory = new Inventory();
    }
}
