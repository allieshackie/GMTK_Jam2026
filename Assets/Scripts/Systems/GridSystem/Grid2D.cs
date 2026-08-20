using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Grid2D : MonoBehaviour
{
    public static Grid2D Instance;
    [SerializeField] private List<GridItemData> _gridItemObjList;
    [SerializeField] private GameObject _gridItemUI;
    [SerializeField] private GameObject _mainGridSection;
    [SerializeField] private GameObject _mainGridContainerSection;

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


    private void Awake()
    {
        Instance = this;

        _playerControls = new Player_Controls();
        _playerControls.UI.RClick.performed += OnRightClick;
        _playerControls.UI.SwapItem.performed += OnSwapItem;
        _playerControls.UI.RotateItem.performed += OnRotateItem;
        _playerControls.UI.Enable();

        Init();
        _selectedGridItemObj = _gridItemObjList[_selectedGridItemObjIndex];
    }

    void Start()
    {
        Canvas inventoryCanvas = GetComponent<Canvas>();
        Player player = FindAnyObjectByType<Player>();

        inventoryCanvas.transform.position = player.transform.position + Vector3.up * 10f;
        inventoryCanvas.transform.localScale = Vector3.one * 0.01f;

        Vector3 direction = inventoryCanvas.transform.position - Camera.main.transform.position;
        inventoryCanvas.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Init()
    {
        _gridArray = new GridObject[_rows, _columns];

        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _gridArray[x,y] = new GridObject( x, y);
            }
        }

        GridLayoutGroup gridLayout = _mainGridSection.GetComponent<GridLayoutGroup>();
        if (gridLayout)
        {
            // This constraint count is specifically "column count", 
            // because the constraint setting in the "Grid Layout Group" is set to "Fixed Column Count"
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = _rows;

            RectTransform rect = _mainGridSection.GetComponent<RectTransform>();
            float menuContentWidth = rect.rect.width;
            float menuContentHeight = rect.rect.height;

            // Get the width/height of the content area in the inventory canvas, and calculate the max cell size that would fully fill that space
            float cellWidth = (menuContentWidth - gridLayout.padding.left - gridLayout.padding.right - gridLayout.spacing.x * (_rows - 1)) / _rows;
            float cellHeight = (menuContentHeight - gridLayout.padding.top - gridLayout.padding.bottom - gridLayout.spacing.y * (_columns - 1)) / _columns;

            // Ideally, keep content area a perfect square so that the cells will perfectly fit, but if the content area is a rectangle, 
            // need to calc based on the shortest size
            //cellSize = Mathf.Min(cellWidth, cellHeight);
            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
            _cellSize = gridLayout.cellSize;
        }

        for (int x = 0; x < _rows; x++)
        {
            for (int y = 0; y < _columns; y++)
            {
                GameObject cellObj = Instantiate(_gridItemUI, _mainGridSection.transform);
                UIGridCell uiCell = cellObj.GetComponent<UIGridCell>();
                if (uiCell)
                {
                    uiCell.Initialize(y, x);
                }
            }
        }

        UIGridCell.OnHoverChanged += HandleGridCellOnHoverChanged;
        UIGridCell.OnCellClick += OnLeftClick;
    }

    private void HandleGridCellOnHoverChanged(UIGridCell cell, bool isHovered)
    {
        if (isHovered)
        {
            _hoveredCell = cell;
        }
        else
        {
            _hoveredCell = null;
        }
    }

    public Quaternion GetPlacedItemRotation()
    {
        return Quaternion.Euler(0, 0, _selectedGridItemObj.GetRotationAngle(_currentDir));
    }

    public Vector2 GetHoveredGridCellPosition()
    {
        if (_hoveredCell)
        {
            Vector2 pos = _hoveredCell.GetComponent<RectTransform>().anchoredPosition;
            pos.x += _cellSize.x * (_selectedGridItemObj.Width * 0.5f);
            pos.y -= _cellSize.y * (_selectedGridItemObj.Height * 0.5f);
            return pos;
        }

        return Vector2.zero;
    }

    private void OnLeftClick()
    {     
        Debug.Log("Left Click");
        Vector2Int selectedCellPos = _hoveredCell.GetXY();
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
            InventoryItem newItem = InventoryItem.Create(_mainGridContainerSection.transform, selectedCellPos, _currentDir, _selectedGridItemObj);
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
        if (_hoveredCell)
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
        if (context.ReadValueAsButton())
        {
            _currentDir = GridItemData.GetNextDir(_currentDir);
        }
    }

    private void OnSwapItem(InputAction.CallbackContext context)
    {
        _selectedGridItemObjIndex++;
        if (_selectedGridItemObjIndex >= _gridItemObjList.Count)
        {
            _selectedGridItemObjIndex = 0;
        }
        _selectedGridItemObj = _gridItemObjList[_selectedGridItemObjIndex];
        OnSelectedGridItemChanged?.Invoke(this, EventArgs.Empty);
    }

    public GridItemData GetGridItemDataType()
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
