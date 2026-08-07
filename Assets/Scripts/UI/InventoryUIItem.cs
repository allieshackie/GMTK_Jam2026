using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _itemName;

    public void InitSlot(InventoryItem data) 
    {
        _image.sprite = data.Item.Icon;
        _itemName.text = data.Item.Name;
    }
}
