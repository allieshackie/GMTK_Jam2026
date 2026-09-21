using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class InventoryItem : MonoBehaviour
{
    public GridItemData Data => _gridItemData;
    public Vector2Int Origin => _origin;
    public GridItemData.Dir Direction => _dir;
    public Grid2D OwnerGrid => _ownerGrid;

    private GridItemData _gridItemData;
    private Vector2Int _origin;
    private GridItemData.Dir _dir;

    private Grid2D _ownerGrid;

    public static InventoryItem Create(Grid2D gridParent, Transform parentTransform, Vector2Int origin, GridItemData.Dir dir, GridItemData data)
    {
        GameObject obj = Instantiate(data.Obj, parentTransform);
        InventoryItem item = obj.GetComponent<InventoryItem>();
        item.SetPlacement(gridParent, parentTransform, origin, dir, data);

        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetPlacement(Grid2D grid, Transform parentTransform, Vector2Int origin, GridItemData.Dir dir, GridItemData data = null)
    {
        if (data != null)
        {
            _gridItemData = data;
        }

        _ownerGrid = grid;
        _origin = origin;
        _dir = dir;

        transform.SetParent(parentTransform, false);

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null || _gridItemData == null)
        {
            return;
        }

        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = grid.GetItemSize(_gridItemData.Width, _gridItemData.Height);
        rectTransform.position = grid.GetItemWorldPosition(origin, _gridItemData, dir);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, _gridItemData.GetRotationAngle(dir));
    }

    public void SetItemSelectable(bool enabled)
    {
        // foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
        // {
        //     graphic.raycastTarget = enabled;
        // }

        Graphic graphic = GetComponent<Graphic>();
        graphic.raycastTarget = enabled;
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return _gridItemData.GetGridPositionList(_origin, _dir);
    }
}
