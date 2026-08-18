using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIGridCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static event Action<UIGridCell, bool> OnHoverChanged;
    public static event Action OnCellClick;
    private Vector2Int _gridPos;

    public void Initialize(int x, int y)
    {
        _gridPos = new Vector2Int(x, y);
    }

    public Vector2Int GetXY()
    {
        return _gridPos;   
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverChanged?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverChanged?.Invoke(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCellClick?.Invoke();
    }
}
