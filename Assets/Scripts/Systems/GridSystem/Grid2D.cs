using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Grid2D : MonoBehaviour
{
    public static Grid2D CurrentlyHoveredGrid { get; private set; }
    public static event Action<Grid2D> OnHoveredGridChanged;
    public event Action<InventoryItem> ItemAddedToGrid;
    public event Action<InventoryItem> ItemMovedWithinGrid;
    public event Action<InventoryItem> ItemRemovedFromGrid;
    public GridItemData SelectedGridItemData => _selectedGridItemObj;

    [SerializeField] private List<GridItemData> _gridItemObjList;
    [SerializeField] private GameObject _gridItemUI;
    [SerializeField] private GameObject _gridCellContainer;
    [SerializeField] private GameObject _gridItemContainer;
    [SerializeField] private GridDragController _dragController;

    [SerializeField] private int _rows = 5;
    [SerializeField] private int _columns = 5;

    public event EventHandler OnSelectedGridItemChanged;
    private GridItemData.Dir _currentDir = GridItemData.Dir.Down;
    private GridItemData _selectedGridItemObj;
    private int _selectedGridItemObjIndex = 0;

    private Player_Controls _playerControls;
    private Vector2 _cellSize;

    private UIGridCell _hoveredCell;

    public class GridObject
    {
        private InventoryItem _item;

        public void SetItem(InventoryItem item)
        {
            _item = item;
        }

        public InventoryItem GetItem()
        {
            return _item;
        }

        public void ClearItem()
        {
            _item = null;
        }

        public bool CanPlace()
        {
            return _item == null;
        }
    }

    private GridObject[,] _gridArray;
    private UIGridCell[,] _uiCells;


    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.UI.RClick.performed += OnRightClick;
        _playerControls.UI.SwapItem.performed += OnSwapItem;
        _playerControls.UI.RotateItem.performed += OnRotateItem;

        Init();
        _dragController = FindAnyObjectByType<GridDragController>();
    }

    private void OnEnable()
    {
        _playerControls?.UI.Enable();
    }

    private void OnDisable()
    {
        _playerControls?.UI.Disable();

        // Note: had a crash if dragging an item -> then closing the inventory, this will make 
        // sure that dragged item is returned to the original grid
        _dragController?.CancelDragFromGrid(this);

        if (CurrentlyHoveredGrid == this)
        {
            SetCurrentlyHoveredGrid(null);
        }

        _hoveredCell = null;
    }

    private void OnDestroy()
    {
        if (_playerControls != null)
        {
            _playerControls.UI.RClick.performed -= OnRightClick;
            _playerControls.UI.SwapItem.performed -= OnSwapItem;
            _playerControls.UI.RotateItem.performed -= OnRotateItem;
            _playerControls.Dispose();
        }

        if (_uiCells == null)
        {
            return;
        }

        foreach (UIGridCell cell in _uiCells)
        {
            if (cell == null)
            {
                continue;
            }

            cell.OnHoverChanged -= HandleGridCellOnHoverChanged;
            cell.OnCellClick -= OnLeftClick;
        }
    }

    private void Init()
    {
        _gridArray = new GridObject[_rows, _columns];

        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _gridArray[x,y] = new GridObject();
            }
        }

        GridLayoutGroup gridLayout = _gridCellContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout)
        {
            // This constraint count is specifically "column count", 
            // because the constraint setting in the "Grid Layout Group" is set to "Fixed Column Count"
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = _rows;

            RectTransform rect = _gridCellContainer.GetComponent<RectTransform>();
            float menuContentWidth = rect.rect.width;
            float menuContentHeight = rect.rect.height;

            // Get the width/height of the content area in the inventory canvas, and calculate the max cell size that would fully fill that space
            float cellWidth = (menuContentWidth - gridLayout.padding.left - gridLayout.padding.right - gridLayout.spacing.x * (_rows - 1)) / _rows;
            float cellHeight = (menuContentHeight - gridLayout.padding.top - gridLayout.padding.bottom - gridLayout.spacing.y * (_columns - 1)) / _columns;

            // Keep cells square and fit the entire grid inside the available content area.
            float cellSize = Mathf.Min(cellWidth, cellHeight);
            gridLayout.cellSize = Vector2.one * cellSize;
            _cellSize = gridLayout.cellSize;
        }

        _uiCells = new UIGridCell[_rows, _columns];
        for (int y = 0; y < _columns; y++)
        {
            for (int x = 0; x < _rows; x++)
            {
                GameObject cellObj = Instantiate(_gridItemUI, _gridCellContainer.transform);
                UIGridCell uiCell = cellObj.GetComponent<UIGridCell>();
                if (uiCell)
                {
                    uiCell.Initialize(x, y);
                    uiCell.OnHoverChanged += HandleGridCellOnHoverChanged;
                    uiCell.OnCellClick += OnLeftClick;
                    _uiCells[x,y] = uiCell;
                }
            }
        }
    }

    private void HandleGridCellOnHoverChanged(UIGridCell cell, bool isHovered)
    {
        if (isHovered)
        {
            _hoveredCell = cell;
            SetCurrentlyHoveredGrid(this);
        }
        else if (_hoveredCell == cell)
        {
            _hoveredCell = null;

            if (CurrentlyHoveredGrid == this)
            {
                SetCurrentlyHoveredGrid(null);
            }
        }
    }

    private static void SetCurrentlyHoveredGrid(Grid2D grid)
    {
        if (CurrentlyHoveredGrid == grid)
        {
            return;
        }

        CurrentlyHoveredGrid = grid;
        OnHoveredGridChanged?.Invoke(grid);
    }

    public Quaternion GetPlacedItemRotation()
    {
        return Quaternion.Euler(0, 0, _selectedGridItemObj.GetRotationAngle(_currentDir));
    }

    public Vector3 GetItemWorldPosition(Vector2Int origin, GridItemData data, GridItemData.Dir direction)
    {
        RectTransform rect = _uiCells[origin.x, origin.y].GetComponent<RectTransform>();
        Vector2Int itemGridSize = GetItemGridSize(data, direction);
        Vector3 itemCenterFromCellTopLeft = new Vector3(rect.rect.xMin + _cellSize.x * itemGridSize.x * 0.5f, rect.rect.yMax - _cellSize.y * itemGridSize.y * 0.5f, 0f);

        return rect.TransformPoint(itemCenterFromCellTopLeft);
    }

    public Vector2 GetHoveredGridCellPosition()
    {
        if (TryGetSelectedCellPos(out Vector2Int selectedCellPos))
        {
            Vector2Int itemGridSize = GetSelectedItemGridSize();
            UIGridCell cell = _uiCells[selectedCellPos.x, selectedCellPos.y];
            Vector2 pos = cell.GetComponent<RectTransform>().anchoredPosition;
            pos.x += _cellSize.x * (itemGridSize.x * 0.5f);
            pos.y -= _cellSize.y * (itemGridSize.y * 0.5f);
            return pos;
        }

        return Vector2.zero;
    }

    public bool TryGetHoveredItemWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (!TryGetSelectedCellPos(out Vector2Int selectedCellPos))
        {
            return false;
        }

        Vector2Int itemGridSize = GetSelectedItemGridSize();
        RectTransform cellRect = _uiCells[selectedCellPos.x, selectedCellPos.y].GetComponent<RectTransform>();
        Vector3 itemCenterFromCellTopLeft = new Vector3(cellRect.rect.xMin + _cellSize.x * itemGridSize.x * 0.5f, cellRect.rect.yMax - _cellSize.y * itemGridSize.y * 0.5f, 0f);

        worldPosition = cellRect.TransformPoint(itemCenterFromCellTopLeft);
        return true;
    }

    public bool TryGetItemWorldPosition(GridItemData data, GridItemData.Dir direction, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (!TryGetHoveredPlacement(data, direction, out Vector2Int origin))
        {
            return false;
        }

        worldPosition = GetItemWorldPosition(origin, data, direction);
        return true;
    }

    private Vector2Int GetSelectedItemGridSize()
    {
        return GetItemGridSize(_selectedGridItemObj, _currentDir);
    }

    private Vector2Int GetItemGridSize(GridItemData data, GridItemData.Dir direction)
    {
        bool isSideways = direction == GridItemData.Dir.Left || direction == GridItemData.Dir.Right;
        if (isSideways)
        {
            return new Vector2Int(data.Height, data.Width);
        }

        return new Vector2Int(data.Width, data.Height);
    }

    private bool TryGetSelectedCellPos(out Vector2Int selectedCellPos)
    {
        selectedCellPos = Vector2Int.zero;
        if (!_hoveredCell || _selectedGridItemObj == null)
        {
            return false;
        }

        Vector2Int itemGridSize = GetSelectedItemGridSize();
        int gridWidth = _gridArray.GetLength(0);
        int gridHeight = _gridArray.GetLength(1);

        if (itemGridSize.x > gridWidth || itemGridSize.y > gridHeight)
        {
            return false;
        }

        Vector2Int hoveredCellPos = _hoveredCell.GetXY();
        selectedCellPos = new Vector2Int(Mathf.Clamp(hoveredCellPos.x, 0, gridWidth - itemGridSize.x), Mathf.Clamp(hoveredCellPos.y, 0, gridHeight - itemGridSize.y));

        return true;
    }

    public bool TryGetHoveredPlacement(GridItemData data, GridItemData.Dir direction, out Vector2Int origin)
    {
        origin = Vector2Int.zero;
        if (!_hoveredCell || data == null)
        {
            return false;
        }

        Vector2Int itemGridSize = GetItemGridSize(data, direction);
        int gridWidth = _gridArray.GetLength(0);
        int gridHeight = _gridArray.GetLength(1);
        if (itemGridSize.x > gridWidth || itemGridSize.y > gridHeight)
        {
            return false;
        }

        Vector2Int hoveredPosition = _hoveredCell.GetXY();
        origin = new Vector2Int(Mathf.Clamp(hoveredPosition.x, 0, gridWidth - itemGridSize.x), Mathf.Clamp(hoveredPosition.y, 0, gridHeight - itemGridSize.y));
        return true;
    }

    public InventoryItem GetItemAt(Vector2Int position)
    {
        if (!IsInBounds(position))
        {
            return null;
        }

        return _gridArray[position.x, position.y].GetItem();
    }

    public bool CanPlace(InventoryItem draggedItem, GridItemData data, Vector2Int origin, GridItemData.Dir direction)
    {
        if (data == null)
        {
            return false;
        }

        foreach (Vector2Int position in data.GetGridPositionList(origin, direction))
        {
            if (!IsInBounds(position))
            {
                return false;
            }

            InventoryItem item = _gridArray[position.x, position.y].GetItem();
            if (item != null && item != draggedItem)
            {
                return false;
            }
        }

        return true;
    }

    public void PlaceExisting(InventoryItem item, Vector2Int origin, GridItemData.Dir direction, bool notify = true)
    {
        foreach (Vector2Int position in item.Data.GetGridPositionList(origin, direction))
        {
            _gridArray[position.x, position.y].SetItem(item);
        }

        item.SetPlacement(this, _gridItemContainer.transform, origin, direction);
        if (notify)
        {
            NotifyItemAdded(item);
        }
    }

    public void ClearItemFromGrid(InventoryItem item, bool notify = true)
    {
        foreach (Vector2Int position in item.GetGridPositionList())
        {
            if (IsInBounds(position) && _gridArray[position.x, position.y].GetItem() == item)
            {
                _gridArray[position.x, position.y].ClearItem();
            }
        }

        if (notify)
        {
            NotifyItemRemoved(item);
        }
    }

    public void ResetItemPosition(InventoryItem item, Vector2Int origin, GridItemData.Dir direction)
    {
        item.SetPlacement(this, _gridItemContainer.transform, origin, direction);
    }

    public void NotifyItemAdded(InventoryItem item)
    {
        ItemAddedToGrid?.Invoke(item);
    }

    public void NotifyItemMoved(InventoryItem item)
    {
        ItemMovedWithinGrid?.Invoke(item);
    } 
    public void NotifyItemRemoved(InventoryItem item)
    {
        ItemRemovedFromGrid?.Invoke(item);
    } 

    private bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < _gridArray.GetLength(0) && position.y < _gridArray.GetLength(1);
    }

    private void OnLeftClick(UIGridCell cell)
    {
        _hoveredCell = cell;
        SetCurrentlyHoveredGrid(this);

        if (_dragController != null && _dragController.IsDragging())
        {
            _dragController.TryDrop(this);
            return;
        }

        InventoryItem clickedItem = GetItemAt(cell.GetXY());
        if (clickedItem != null && _dragController != null)
        {
            _dragController.BeginDrag(clickedItem);
            return;
        }

        if (!TryGetSelectedCellPos(out Vector2Int selectedCellPos))
        {
            return;
        }

        bool canPlace = true;
        List<Vector2Int> posList = _selectedGridItemObj.GetGridPositionList(selectedCellPos, _currentDir);
        foreach(Vector2Int vec in posList)
        {
            GridObject obj = _gridArray[vec.x, vec.y];
            if (obj == null || !obj.CanPlace())
            {
                canPlace = false;
                break;
            }
        }
        if (canPlace)
        {
            InventoryItem newItem = InventoryItem.Create(this, _gridItemContainer.transform, selectedCellPos, _currentDir, _selectedGridItemObj);
            foreach (Vector2Int vec in posList)
            {
                _gridArray[vec.x, vec.y].SetItem(newItem);
            }
            NotifyItemAdded(newItem);
        }
        else
        {
            Debug.Log("Can't place here");
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (CurrentlyHoveredGrid == this && _dragController != null && _dragController.IsDragging())
        {
            _dragController.CancelDrag();
            return;
        }

        if (CurrentlyHoveredGrid == this && _hoveredCell)
        {
            GridObject obj = _gridArray[_hoveredCell.GetXY().x, _hoveredCell.GetXY().y];
            InventoryItem item = obj.GetItem();
            if (item != null)
            {
                List<Vector2Int> posList = item.GetGridPositionList();
                foreach(Vector2Int vec in posList)
                {
                    GridObject setObj = _gridArray[vec.x, vec.y];
                    if (setObj != null)
                    {   
                        setObj.ClearItem();
                    }
                }
                
                item.DestroySelf();
                NotifyItemRemoved(item);
            }
        }
    }

    private void OnRotateItem(InputAction.CallbackContext context)
    {
        if (CurrentlyHoveredGrid == this && context.ReadValueAsButton())
        {
            if (_dragController != null && _dragController.IsDragging())
            {
                _dragController.RotateDraggedItem();
                return;
            }

            _currentDir = GridItemData.GetNextDir(_currentDir);
        }
    }

    private void OnSwapItem(InputAction.CallbackContext context)
    {
        if (CurrentlyHoveredGrid != this)
        {
            return;
        }

        if (_gridItemObjList.Count == 0)
        {
            return;
        }

        _selectedGridItemObjIndex++;
        if (_selectedGridItemObjIndex >= _gridItemObjList.Count)
        {
            _selectedGridItemObjIndex = 0;
        }
        _selectedGridItemObj = _gridItemObjList[_selectedGridItemObjIndex];
        OnSelectedGridItemChanged?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetItemSize(int width, int height)
    {
        float calcWidth = width * _cellSize.x;
        float calcHeight = height * _cellSize.y;

        return new Vector2(calcWidth, calcHeight);
    }

    public void InitWithItems(List<PendingItem> pendingItems)
    {
        RectTransform cellContainerRect = _gridCellContainer.transform as RectTransform;
        if (cellContainerRect != null)
        {
            // Note: The placed items were weirdly offset and it was apparently a timing issue of the 
            // grid adding the cells and the item placing itself.  
            // Force the canvas to rebuild if it's not ready
            LayoutRebuilder.ForceRebuildLayoutImmediate(cellContainerRect);
        }

        foreach(PendingItem item in pendingItems)
        {
            bool canPlace = true;
            List<Vector2Int> posList = item.Data.GetGridPositionList(item.Position, item.Dir);
            foreach(Vector2Int vec in posList)
            {
                GridObject obj = _gridArray[vec.x, vec.y];
                if (obj == null || !obj.CanPlace())
                {
                    canPlace = false;
                    break;
                }
            }
            if (canPlace)
            {
                InventoryItem newItem = InventoryItem.Create(this, _gridItemContainer.transform, item.Position, item.Dir, item.Data);
                foreach (Vector2Int vec in posList)
                {
                    _gridArray[vec.x, vec.y].SetItem(newItem);
                }
                NotifyItemAdded(newItem);
            }
        }
    }

    public List<PendingItem> GetCurrentPendingItems()
    {
        // Since items can take up multiple cells, don't want to
        // double up and count the same item
        HashSet<InventoryItem> foundItems = new();
        List<PendingItem> items = new();

        foreach (GridObject cell in _gridArray)
        {
            InventoryItem item = cell.GetItem();
            if (item == null || !foundItems.Add(item))
            {
                continue;
            }

            items.Add(new PendingItem{ Data = item.Data, Position = item.Origin, Dir = item.Direction});
        }

        return items;
    }

    public void ClearGrid()
    {
        _gridArray = new GridObject[_rows, _columns];

        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _gridArray[x,y] = new GridObject();
            }
        }
    }
}
