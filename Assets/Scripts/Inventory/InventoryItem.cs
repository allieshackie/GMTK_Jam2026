[System.Serializable]

public class InventoryItem
{
    public ItemData Item;
    public int Count;

    public InventoryItem(ItemData item)
    {
        Item = item;
        Count = 1;
    }
}
