using UnityEngine;
using Manaflow.Systems;
using UnityEngine.InputSystem;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private Sprite _testSprite;
    private Grid<GridObject> _grid;

    private Player_Controls _playerControls;
    private Camera _mainCam;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.Player.Attack.performed += OnLeftClick;
        //_playerControls.Player.RClick.performed += OnRightClick;
        _playerControls.Player.Enable();

        int gridWidth = 8;
        int gridHeight = 5;
        float cellSize = 8f;
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

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            _grid = grid;
            _x = x;
            _y = y;
        }

        public override string ToString()
        {
            return _x + ", " + _y;
        }
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            var mouseInWorld = DebugUtils.MouseInWorldSpace(_mainCam);
            Vector3 worldPosition = new Vector3(mouseInWorld.x, mouseInWorld.y, 0);
            Instantiate(_testSprite, )
        }
    }
    
}
