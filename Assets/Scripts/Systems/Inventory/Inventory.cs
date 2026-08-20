using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<InventoryItem, int> _items = new();

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
