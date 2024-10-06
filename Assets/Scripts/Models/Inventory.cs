using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class Inventory
{
    public Dictionary<InventoryItem, int> Items;

    public Inventory()
    {
        // Initialise an inventory with all items set to zero
        Items = new Dictionary<InventoryItem, int>();
        foreach (InventoryItem item in Enum.GetValues(typeof(InventoryItem)))
        {
           Items.Add(item, 0);
        }
    }

    // Check whether the provided inventory can be "afforded"
    public bool Check(Inventory inventory)
    {
        // Loop through each item in the inventory and see if it can be afforded
        foreach (InventoryItem item in inventory.Items.Keys)
        {
            if (Items[item] > inventory.Items[item]) return false;
        }
        return true;
    }

    public void Spend(Inventory inventory)
    {
        // If this can be afforded
        if (Check(inventory))
        {
            // Loop through each item in the inventory and remove
            foreach (InventoryItem item in inventory.Items.Keys)
            {
                Items[item] -= inventory.Items[item];
            }
        }
    }

    public void Add(InventoryItem item, int amount)
    {
        // Add the provided amount to an item
        Items[item] += amount;
    }
}