using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;

    [SerializeField] private TextMeshProUGUI _woodText;
    [SerializeField] private TextMeshProUGUI _stoneText;
    [SerializeField] private TextMeshProUGUI _planksText;
    [SerializeField] private TextMeshProUGUI _blocksText;

    // Start is called before the first frame update
    void Start()
    {
        _inventory.OnInventoryUpdated += UpdateDisplay;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        _woodText.text = $"Wood: {_inventory.Items[InventoryItem.Wood]}";
        _stoneText.text = $"Stone: {_inventory.Items[InventoryItem.Stone]}";
        _planksText.text = $"Planks: {_inventory.Items[InventoryItem.Planks]}";
        _blocksText.text = $"Blocks: {_inventory.Items[InventoryItem.Blocks]}";

    }
}
