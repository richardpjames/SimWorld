using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<InventoryItem, int> Items;
    public Action OnInventoryUpdated;

    private void Start()
    {
        // Initialise an inventory with all items set to zero
        Items = new Dictionary<InventoryItem, int>();
        foreach (InventoryItem item in Enum.GetValues(typeof(InventoryItem)))
        {
           Items.Add(item, 0);
        }
        // Add starting resources
        Add(GameManager.Instance.StartingResources);
        OnInventoryUpdated?.Invoke();
    }

    // Check whether the provided inventory can be "afforded"
    public bool Check(Dictionary<InventoryItem, int> inventory)
    {
        if (inventory == null) return true;
        // Loop through each item in the inventory and see if it can be afforded
        foreach (InventoryItem item in inventory.Keys)
        {
            if (Items[item] < inventory[item]) return false;
        }
        return true;
    }

    public void Spend(Dictionary<InventoryItem, int> inventory)
    {
        if (inventory == null) return;
        // If this can be afforded
        if (Check(inventory))
        {
            // Loop through each item in the inventory and remove
            foreach (InventoryItem item in inventory.Keys)
            {
                Items[item] -= inventory[item];
            }
        }
        OnInventoryUpdated?.Invoke();
    }
    public void Add(Dictionary<InventoryItem, int> inventory)
    {
        if (inventory == null) return;
        // Loop through each item in the inventory and add
        foreach (InventoryItem item in inventory.Keys)
        {
            Items[item] += inventory[item];
        }
        OnInventoryUpdated?.Invoke();
    }
}