using UnityEngine;
using UnityEngine.InputSystem;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class GridItemGhost : MonoBehaviour
{
    [SerializeField] private Grid2D _gridParent;
    private GameObject _currentVisual;
    void Start()
    {
        // Note: this is mainly for testing, where you can swap item types
        // Normally you'll be selecting one item at a time and placing them
        // Uncomment for testing
        RefreshVisual();
        _gridParent.OnSelectedGridItemChanged += OnSelectedChanged;
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
        
        GridItemData data = _gridParent.GetGridItemDataType();
        if (data != null)
        {
            _currentVisual = Instantiate(data.Obj, transform);
            RectTransform rectTransform = _currentVisual.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localPosition = Vector3.zero;
                rectTransform.sizeDelta = _gridParent.GetItemSize(data.Width, data.Height);
            }
        }
    }

    private void LateUpdate()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 targetPosition = _gridParent.GetHoveredGridCellPosition();

        if (targetPosition != Vector2.zero)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * 15f);
        }
        else
        {
            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (Mouse.current != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Mouse.current.position.ReadValue(), Camera.main, out Vector2 localMousePos))
            {
                rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, localMousePos, Time.deltaTime * 25f);
            }
        }

        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, _gridParent.GetPlacedItemRotation(), Time.deltaTime * 15f);
    }
}
