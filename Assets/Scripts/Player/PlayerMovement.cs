using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.6f;
    [SerializeField] private float _accesleration = 12f;
    [SerializeField] private float _isoAngle;

    private Rigidbody _rb;
    private Collider _collider;

    private Player_Controls _playerControls;

    private Vector2 _inputVector;
    private Vector3 _targetVelocity;

    private void Awake()
    {
        _playerControls = new Player_Controls();
    }
    
    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();

        _playerControls.Player.Movement.performed += OnMove;
        _playerControls.Player.Movement.canceled += OnMove;

        _playerControls.Player.Movement.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Movement.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        print($"Movement Input: {context.ReadValue<Vector2>()}");
        _inputVector = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        
    }
}
