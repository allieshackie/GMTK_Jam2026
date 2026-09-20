using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Grid2D : MonoBehaviour
{
    public static Grid2D CurrentlyHoveredGrid { get; private set; }
    public static event Action<Grid2D> OnHoveredGridChanged;

    [SerializeField] private List<GridItemData> _gridItemObjList;
    [SerializeField] private GameObject _gridItemUI;
    [SerializeField] private GameObject _gridCellContainer;
    [SerializeField] private GameObject _gridItemContainer;

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
        private int _x, _y;
        private InventoryItem _item;

        public GridObject(int x, int y)
        {
            _x = x;
            _y = y;
        }

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
        _selectedGridItemObj = _gridItemObjList[_selectedGridItemObjIndex];
    }

    private void OnEnable()
    {
        _playerControls?.UI.Enable();
    }

    private void OnDisable()
    {
        _playerControls?.UI.Disable();

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
                _gridArray[x,y] = new GridObject(x, y);
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

            // Ideally, keep content area a perfect square so that the cells will perfectly fit, but if the content area is a rectangle, 
            // need to calc based on the shortest size
            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
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

    private Vector2Int GetSelectedItemGridSize()
    {
        bool isSideways = _currentDir == GridItemData.Dir.Left || _currentDir == GridItemData.Dir.Right;
        if (isSideways)
        {
            return new Vector2Int(_selectedGridItemObj.Height, _selectedGridItemObj.Width);
        }

        return new Vector2Int(_selectedGridItemObj.Width, _selectedGridItemObj.Height);
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

    private void OnLeftClick(UIGridCell cell)
    {
        _hoveredCell = cell;
        SetCurrentlyHoveredGrid(this);

        if (!TryGetSelectedCellPos(out Vector2Int selectedCellPos))
        {
            return;
        }

        bool canBuild = true;
        List<Vector2Int> posList = _selectedGridItemObj.GetGridPositionList(selectedCellPos, _currentDir);
        foreach(Vector2Int vec in posList)
        {
            GridObject obj = _gridArray[vec.x, vec.y];
            if (obj == null || !obj.CanPlace())
            {
                canBuild = false;
                break;
            }
        }
        if (canBuild)
        {
            InventoryItem newItem = InventoryItem.Create(this, _gridItemContainer.transform, selectedCellPos, _currentDir, _selectedGridItemObj);
            foreach (Vector2Int vec in posList)
            {
                _gridArray[vec.x, vec.y].SetItem(newItem);
            }
        }
        else
        {
            Debug.Log("Can't place here");
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
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
            }
        }
    }

    private void OnRotateItem(InputAction.CallbackContext context)
    {
        if (CurrentlyHoveredGrid == this && context.ReadValueAsButton())
        {
            _currentDir = GridItemData.GetNextDir(_currentDir);
        }
    }

    private void OnSwapItem(InputAction.CallbackContext context)
    {
        if (CurrentlyHoveredGrid != this)
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

    public GridItemData GetSelectedGridItemData()
    {
        return _selectedGridItemObj;
    }

    public Vector2 GetItemSize(int width, int height)
    {
        float calcWidth = width * _cellSize.x;
        float calcHeight = height * _cellSize.y;

        return new Vector2(calcWidth, calcHeight);
    }
}
