using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class GridItemGhost : MonoBehaviour
{
    private Grid2D _currentlyHoveredGrid;
    private Grid2D _displayGrid;
    private GameObject _currentVisual;
    private GridDragController _dragController;

    void Start()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            _dragController = parentCanvas.GetComponentInChildren<GridDragController>(true);
        }

        Graphic ghostLayerGraphic = transform.parent.GetComponent<Graphic>();
        if (ghostLayerGraphic != null)
        {
            ghostLayerGraphic.raycastTarget = false;
        }

        Grid2D.OnHoveredGridChanged += HandleHoveredGridChanged;
        HandleHoveredGridChanged(Grid2D.CurrentlyHoveredGrid);
    }

    private void OnDestroy()
    {
        Grid2D.OnHoveredGridChanged -= HandleHoveredGridChanged;
        if (_currentlyHoveredGrid != null)
        {
            _currentlyHoveredGrid.OnSelectedGridItemChanged -= OnSelectedChanged;
        }
    }

    private void HandleHoveredGridChanged(Grid2D grid)
    {
        if (_currentlyHoveredGrid != null)
        {
            _currentlyHoveredGrid.OnSelectedGridItemChanged -= OnSelectedChanged;
        }

        _currentlyHoveredGrid = grid;

        if (_currentlyHoveredGrid != null)
        {
            _displayGrid = _currentlyHoveredGrid;
            _currentlyHoveredGrid.OnSelectedGridItemChanged += OnSelectedChanged;
            RefreshVisual();
        }
    }

    private void OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_currentVisual != null)
        {
            Destroy(_currentVisual);
            _currentVisual = null;
        }
        
        if (_displayGrid == null)
        {
            return;
        }

        GridItemData data = _displayGrid.SelectedGridItemData;
        if (data != null)
        {
            _currentVisual = Instantiate(data.Obj, transform);
            RectTransform rectTransform = _currentVisual.GetComponent<RectTransform>();

            foreach (Graphic graphic in _currentVisual.GetComponentsInChildren<Graphic>())
            {
                graphic.raycastTarget = false;
            }

            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localPosition = Vector3.zero;
                rectTransform.sizeDelta = _displayGrid.GetItemSize(data.Width, data.Height);
            }
        }
    }

    private void LateUpdate()
    {
        if (_currentVisual != null)
        {
            _currentVisual.SetActive(!_dragController.IsDragging());
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform parentRect = rectTransform.parent as RectTransform;

        if (_currentlyHoveredGrid != null && parentRect != null && _currentlyHoveredGrid.TryGetHoveredItemWorldPosition(out Vector3 targetWorldPosition))
        {
            Vector3 targetLocalPosition = parentRect.InverseTransformPoint(targetWorldPosition);
            rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetLocalPosition, Time.deltaTime * 15f);
        }
        else
        {
            if (parentRect != null && Mouse.current != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Mouse.current.position.ReadValue(), Camera.main, out Vector2 localMousePos))
            {
                rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, localMousePos, Time.deltaTime * 25f);
            }
        }

        if (_displayGrid != null)
        {
            //rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, _displayGrid.GetPlacedItemRotation(), Time.deltaTime * 15f);
        }
    }
}
