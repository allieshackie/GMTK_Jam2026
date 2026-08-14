using Manaflow.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridTestScript : MonoBehaviour
{
    private Grid<int> _mGrid;
    private Player_Controls _playerControls;
    private Camera _mainCam;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.Player.Attack.performed += OnLeftClick;
        _playerControls.Player.RClick.performed += OnRightClick;
        _playerControls.Player.Enable();
    }

    void Start()
    {
        _mainCam = Camera.main;
        _mGrid = new Grid<int>(2,4,5f, new Vector3(20,0), (Grid<int> g, int x, int y) => 1);
        Debug.Log("Test");
    }

    private void Update()
    {
    }

    // LClick binding.
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            var mouseInWorld = DebugUtils.MouseInWorldSpace(_mainCam);
            Vector3 worldPosition = new Vector3(mouseInWorld.x, mouseInWorld.y, 0);
            _mGrid.SetValue(worldPosition, 56);
        }
    }

    // RClick binding.
    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Debug.Log(_mGrid.GetValue(DebugUtils.MouseInWorldSpace(_mainCam)));
        }
    }
}
