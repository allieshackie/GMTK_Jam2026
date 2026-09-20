using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class InventoryItem : MonoBehaviour
{
    private GridItemData _gridItemData;
    private Vector2Int _origin;
    private GridItemData.Dir _dir;

    public static InventoryItem Create(Grid2D gridParent, Transform parentTransform, Vector2Int origin, GridItemData.Dir dir, GridItemData data)
    {
        GameObject obj = Instantiate(data.Obj, parentTransform);
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = gridParent.GetHoveredGridCellPosition();
        rectTransform.sizeDelta = gridParent.GetItemSize(data.Width, data.Height);
        rectTransform.localRotation = gridParent.GetPlacedItemRotation();

        InventoryItem item = obj.GetComponent<InventoryItem>();
        item._gridItemData = data;
        item._origin = origin;
        item._dir = dir;

        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return _gridItemData.GetGridPositionList(_origin, _dir);
    }
}
