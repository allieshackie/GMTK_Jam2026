using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private List<PendingItem> _items;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            Inventory inventory = FindAnyObjectByType<Inventory>();
            inventory.OpenPendingGrid(this);
        }
    }

    public List<PendingItem> GetPendingItems()
    {
        return _items;
    }

    public void SavePendingItems(List<PendingItem> items)
    {
        _items = items;
    }
}
