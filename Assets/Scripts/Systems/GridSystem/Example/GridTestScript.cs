using FMODUnity;
using Manaflow.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using MGrid = Manaflow.Systems.Grid;

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
        _mGrid = new MGrid(2,4,5f, new Vector3(20,0));
    }

    private void Update()
    {
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Vector3 worldPosition = new Vector3(MouseInWorldSpace().x, MouseInWorldSpace().y, 0);
            _mGrid.SetValue(worldPosition, 56);
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Debug.Log(_mGrid.GetValue(MouseInWorldSpace()));
        }
    }

    // Add this to utils.
    private Vector3 MouseInWorldSpace()
    {
        return new Vector3( _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x, _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()).y);
    }
}
