using UnityEngine;
using Manaflow.Systems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private GridItemData _testObj;

    private Grid<GridObject> _grid;
    private GridItemData.Dir _currentDir = GridItemData.Dir.Down;

    private Player_Controls _playerControls;
    private Camera _mainCam;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.Player.Attack.performed += OnLeftClick;
        _playerControls.Player.RClick.performed += OnRightClick;
        _playerControls.Player.Enable();

        int gridWidth = 8;
        int gridHeight = 5;
        float cellSize = 10f;
        _grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0), (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));
    }
    void Start()
    {
        _mainCam = Camera.main;
    }
    public class GridObject
    {
        private Grid<GridObject> _grid;
        private int _x, _y;
        private Transform _transform;

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            _grid = grid;
            _x = x;
            _y = y;
        }

        public void SetTransform(Transform transform)
        {
            _transform = transform;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public void ClearTransform()
        {
            _transform = null;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public bool CanPlace()
        {
            return _transform == null;
        }

        public override string ToString()
        {
            return _x + ", " + _y + "\n" + _transform;
        }
    }
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            var mouseInWorld = DebugUtils.MouseInWorldSpace(_mainCam);
            _grid.GetXY(mouseInWorld, out int x, out int y);
     
            bool canBuild = true;
            List<Vector2Int> posList = _testObj.GetGridPositionList(new Vector2Int(x, y), _currentDir);
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
                Vector2Int rotationOffset = _testObj.GetRotationOffset(_currentDir);
                Vector3 placedPos = _grid.GetWorldPosition(x, y) + (new Vector3(rotationOffset.x, rotationOffset.y, 0) * _grid.GetCellSize());
                GameObject newSprite = Instantiate(_testObj.Obj, placedPos, Quaternion.Euler(0, 0, _testObj.GetRotationAngle(_currentDir)));
                foreach (Vector2Int vec in posList)
                {
                    _grid.GetGridObject(vec.x, vec.y).SetTransform(newSprite.transform);
                }
            }
            else
            {
                Debug.Log("Can't place here");
            }
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            _currentDir = GridItemData.GetNextDir(_currentDir);
        }
    }
    
}
