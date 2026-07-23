using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.6f;
    [SerializeField] private float _acceleration = 12f;

    [Tooltip("This value must match the camera yaw angle")]
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
        // 1. Rotate raw input into isometric space
        float rad = _isoAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float isoX = _inputVector.x * cos - _inputVector.y * sin;
        float isoZ = _inputVector.x * sin + _inputVector.y * cos;

        Vector3 moveDir = new Vector3(isoX, 0f, isoZ);

        // Prevent diagonal input (e.g. (1,1)) from moving faster than a single axis
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // 2. Compute desired velocity
        Vector3 desiredVelocity = moveDir * _moveSpeed;
        desiredVelocity.y = _rb.linearVelocity.y; // preserve gravity/vertical velocity

        // 3. Smooth toward it (accel/decel feel)
        _targetVelocity = Vector3.Lerp(_rb.linearVelocity, desiredVelocity, _acceleration * Time.fixedDeltaTime);

        _rb.linearVelocity = _targetVelocity;
    }
}
