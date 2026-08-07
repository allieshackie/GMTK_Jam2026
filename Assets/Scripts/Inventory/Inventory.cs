using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<ItemData> _allItemData;
    private List<InventoryItem> _itemsList = new List<InventoryItem>();

    private InventoryUI _inventoryUI;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inventoryUI = FindAnyObjectByType<InventoryUI>();
        
        Init();
    }

    void Init()
    {
        for (int i = 0; i < _allItemData.Count; i++) 
        {
            // TODO: Check here if item is locked before adding?
            _itemsList.Add(new InventoryItem(_allItemData[i]));
        }

        _inventoryUI.UpdateGrid(_itemsList);
    }
}
