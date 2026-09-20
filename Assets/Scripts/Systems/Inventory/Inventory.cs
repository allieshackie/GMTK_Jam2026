using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryUI;
    private Dictionary<InventoryItem, int> _items = new();

    private Player_Controls _playerControls;

    private bool _isInventoryOpen = false;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.UI.ToggleInventory.performed += ToggleInventory;
        _playerControls.UI.Enable();
    }

    private void Start()
    {
        _inventoryUI.SetActive(_isInventoryOpen);
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        _isInventoryOpen = !_isInventoryOpen;
        _inventoryUI.SetActive(_isInventoryOpen);
    }

    public void Add(InventoryItem item, int amount)
    {
        if (!_items.ContainsKey(item))
        {   
            _items[item] = 0;
        }

        _items[item] += amount;
    }

    public void Remove(InventoryItem ingredient, int amount)
    {
        if (_items.ContainsKey(ingredient))
        {   
            _items[ingredient] -= amount;
        }
    }

    public bool Has(InventoryItem ingredient, int amount)
    {
        if (_items.ContainsKey(ingredient))
        {
            return _items[ingredient] >= amount;
        }

        return false;
    }

}
