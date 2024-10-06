using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<InventoryItem, int> Items;

    private void Start()
    {
        // Initialise an inventory with all items set to zero
        Items = new Dictionary<InventoryItem, int>();
        foreach (InventoryItem item in Enum.GetValues(typeof(InventoryItem)))
        {
           Items.Add(item, 0);
        }
    }

    // Check whether the provided inventory can be "afforded"
    public bool Check(Dictionary<InventoryItem, int> inventory)
    {
        // Loop through each item in the inventory and see if it can be afforded
        foreach (InventoryItem item in inventory.Keys)
        {
            if (Items[item] > inventory[item]) return false;
        }
        return true;
    }

    public void Spend(Dictionary<InventoryItem, int> inventory)
    {
        // If this can be afforded
        if (Check(inventory))
        {
            // Loop through each item in the inventory and remove
            foreach (InventoryItem item in inventory.Keys)
            {
                Items[item] -= inventory[item];
            }
        }
    }
    public void Add(InventoryItem item, int amount)
    {
        // Add the provided amount to an item
        Items[item] += amount;
    }
}