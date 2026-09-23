using UnityEngine;
using UnityEngine.InputSystem;

public class GridDragController : MonoBehaviour
{
    private InventoryItem _draggedItem;
    private Grid2D _sourceGrid;
    private Vector2Int _itemOrigin;
    private GridItemData.Dir _itemDirection;
    private GridItemData.Dir _draggedDirection;

    private void LateUpdate()
    {
        if (_draggedItem == null)
        {
            return;
        }

        RectTransform itemRect = _draggedItem.GetComponent<RectTransform>();
        if (itemRect == null)
        {
            return;
        }

        Grid2D hoveredGrid = Grid2D.CurrentlyHoveredGrid;
        if (hoveredGrid != null && hoveredGrid.TryGetItemWorldPosition(_draggedItem.Data, _draggedDirection, out Vector3 gridWorldPosition))
        {
            itemRect.position = gridWorldPosition;
            itemRect.sizeDelta = hoveredGrid.GetItemSize(_draggedItem.Data.Width, _draggedItem.Data.Height);
        }
        else if (Mouse.current != null)
        {
            RectTransform dragLayer = transform as RectTransform;
            if (dragLayer != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(dragLayer, Mouse.current.position.ReadValue(), Camera.main, out Vector3 mouseWorldPosition))
            {
                itemRect.position = mouseWorldPosition;
            }
        }

        itemRect.rotation = transform.rotation * Quaternion.Euler(0f, 0f, _draggedItem.Data.GetRotationAngle(_draggedDirection));
    }
    
    public bool IsDragging()
    {
        return _draggedItem != null;
    }

    public bool BeginDrag(InventoryItem item)
    {
        if (item == null || IsDragging() || item.OwnerGrid == null)
        {
            return false;
        }

        _draggedItem = item;
        _sourceGrid = item.OwnerGrid;
        _itemOrigin = item.Origin;
        _itemDirection = item.Direction;
        _draggedDirection = item.Direction;

        transform.SetAsLastSibling();
        item.transform.SetParent(transform, true);
        item.SetItemSelectable(false);
        item.transform.SetAsLastSibling();
        return true;
    }

    public bool TryDrop(Grid2D targetGrid)
    {
        if (_draggedItem == null || targetGrid == null)
        {
            return false;
        }

        if (!targetGrid.TryGetHoveredPlacement(_draggedItem.Data, _draggedDirection, out Vector2Int targetOrigin))
        {
            return false;
        }

        if (!targetGrid.CanPlace(_draggedItem, _draggedItem.Data, targetOrigin, _draggedDirection))
        {
            return false;
        }

        Grid2D previousGrid = _sourceGrid;
        previousGrid.ClearItemFromGrid(_draggedItem, false);

        if (previousGrid == targetGrid)
        {
            targetGrid.PlaceExisting(_draggedItem, targetOrigin, _draggedDirection, false);
            targetGrid.NotifyItemMoved(_draggedItem);
        }
        else
        {
            previousGrid.NotifyItemRemoved(_draggedItem);
            targetGrid.PlaceExisting(_draggedItem, targetOrigin, _draggedDirection, false);
            targetGrid.NotifyItemAdded(_draggedItem);
        }

        FinishDrag();
        return true;
    }

    public void RotateDraggedItem()
    {
        if (_draggedItem != null)
        {
            _draggedDirection = GridItemData.GetNextDir(_draggedDirection);
        }
    }

    public void CancelDrag()
    {
        if (_draggedItem == null)
        {
            return;
        }

        if (_sourceGrid != null)
        {
            _sourceGrid.ResetItemPosition(_draggedItem, _itemOrigin, _itemDirection);
        }

        FinishDrag();
    }

    public void CancelDragFromGrid(Grid2D sourceGrid)
    {
        if (_draggedItem != null && _sourceGrid == sourceGrid)
        {
            CancelDrag();
        }
    }

    private void FinishDrag()
    {
        if (_draggedItem != null)
        {
            _draggedItem.SetItemSelectable(false);
        }

        _draggedItem = null;
        _sourceGrid = null;
    }
}
