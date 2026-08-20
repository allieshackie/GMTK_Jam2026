using UnityEngine;
using Manaflow.Systems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance;

    [SerializeField] private List<GridItemData> _gridItemObjList;
    public event EventHandler OnSelectedGridItemChanged;

    private Grid<GridObject> _grid;
    private GridItemData.Dir _currentDir = GridItemData.Dir.Down;
    private GridItemData _selectedGridItemObj;
    private int _selectedGridItemObjIndex = 0;

    private Player_Controls _playerControls;
    private Camera _mainCam;

    private void Awake()
    {
        Instance = this;

        _playerControls = new Player_Controls();
        _playerControls.UI.Select.performed += OnLeftClick;
        _playerControls.UI.RClick.performed += OnRightClick;
        _playerControls.UI.SwapItem.performed += OnSwapItem;
        _playerControls.UI.RotateItem.performed += OnRotateItem;
        _playerControls.UI.Enable();

        int gridWidth = 8;
        int gridHeight = 5;
        float cellSize = 10f;
        _grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0), (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));
        
        _selectedGridItemObj = _gridItemObjList[_selectedGridItemObjIndex];
    }

    void Start()
    {
        _mainCam = Camera.main;
    }

    public class GridObject
    {
        private Grid<GridObject> _grid;
        private int _x, _y;
        private InventoryItem _item;

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            _grid = grid;
            _x = x;
            _y = y;
        }

        public void SetItem(InventoryItem item)
        {
            _item = item;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public InventoryItem GetItem()
        {
            return _item;
        }

        public void ClearItem()
        {
            _item = null;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public bool CanPlace()
        {
            return _item == null;
        }

        public override string ToString()
        {
            return _x + ", " + _y + "\n" + _item;
        }
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            _grid.GetXY(GridUtils.MouseInWorldSpace(_mainCam), out int x, out int y);
     
            bool canBuild = true;
            List<Vector2Int> posList = _selectedGridItemObj.GetGridPositionList(new Vector2Int(x, y), _currentDir);
            foreach(Vector2Int vec in posList)
            {
                GridObject obj = _grid.GetGridObject(vec.x, vec.y);
                if (obj == null || !obj.CanPlace())
                {
                    canBuild = false;
                    break;
                }
            }
            if (canBuild)
            {
                Vector2Int rotationOffset = _selectedGridItemObj.GetRotationOffset(_currentDir);
                Vector3 placedPos = _grid.GetWorldPosition(x, y) + (new Vector3(rotationOffset.x, rotationOffset.y, 0) * _grid.GetCellSize());
                //InventoryItem newItem = InventoryItem.Create(placedPos, new Vector2Int(x,y), _currentDir, _selectedGridItemObj);
                // foreach (Vector2Int vec in posList)
                // {
                //     _grid.GetGridObject(vec.x, vec.y).SetItem(newItem);
                // }
            }
            else
            {
                Debug.Log("Can't place here");
            }
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        GridObject gridObject = _grid.GetGridObject(GridUtils.MouseInWorldSpace(_mainCam));
        InventoryItem item = gridObject.GetItem();
        if (item != null)
        {
            List<Vector2Int> posList = item.GetGridPositionList();
            foreach(Vector2Int vec in posList)
            {
                GridObject obj = _grid.GetGridObject(vec.x, vec.y);
                if (obj != null)
                {   
                    obj.ClearItem();
                }
            }
            
            item.DestroySelf();
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

    public Vector3 GetMouseWorldSnappedPosition()
    {
        _grid.GetXY(GridUtils.MouseInWorldSpace(_mainCam), out int x, out int y);
        Vector2Int rotationOffset = _selectedGridItemObj.GetRotationOffset(_currentDir);
        return _grid.GetWorldPosition(x, y) + (new Vector3(rotationOffset.x, rotationOffset.y, 0) * _grid.GetCellSize());
    }

    public Quaternion GetPlacedItemRotation()
    {
        return Quaternion.Euler(0, 0, _selectedGridItemObj.GetRotationAngle(_currentDir));
    }
    
}
