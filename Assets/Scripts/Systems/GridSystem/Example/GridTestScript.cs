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
        Debug.Log("Test");
    }

    private void Update()
    {
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Vector3 worldPosition = new Vector3(DebugUtils.MouseInWorldSpace(_mainCam).x, DebugUtils.MouseInWorldSpace(_mainCam).y, 0);
            _mGrid.SetValue(worldPosition, 56);
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Debug.Log(_mGrid.GetValue(DebugUtils.MouseInWorldSpace(_mainCam)));
        }
    }
}
