using UnityEngine;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class GridItemGhost : MonoBehaviour
{
    private GameObject _currentVisual;
    void Start()
    {
        RefreshVisual();

        Grid2D.Instance.OnSelectedGridItemChanged += OnSelectedChanged;
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
        
        GridItemData data = Grid2D.Instance.GetGridItemDataType();
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
                rectTransform.sizeDelta = Grid2D.Instance.GetItemSize(data.Width, data.Height);
            }
        }
    }

    private void LateUpdate()
    {
        Vector2 targetPosition = Grid2D.Instance.GetHoveredGridCellPosition();
        if (targetPosition != Vector2.zero)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * 15f);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, Grid2D.Instance.GetPlacedItemRotation(), Time.deltaTime * 15f);
        }
    }
}
