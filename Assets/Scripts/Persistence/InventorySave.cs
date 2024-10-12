using System;

[Serializable]
public class InventorySave
{
    public InventoryItemSave[] Items;
}

[Serializable]
public struct InventoryItemSave
{
    public int Item;
    public int Amount;
}