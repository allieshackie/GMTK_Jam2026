using FMODUnity;
using Manaflow.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using MGrid = Manaflow.Systems.Grid; // assigning an alias because unity has its own "Grid" system.

public class GridTestScript : MonoBehaviour
{
    private MGrid _mGrid;
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

        // Here's where i initialize it for testing.
        _mGrid = new MGrid(2,4,5f, new Vector3(20,0));
    }

    private void Update()
    {
    }

    // LClick binding.
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Vector3 worldPosition = new Vector3(DebugUtils.MouseInWorldSpace(_mainCam).x, DebugUtils.MouseInWorldSpace(_mainCam).y, 0);
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
