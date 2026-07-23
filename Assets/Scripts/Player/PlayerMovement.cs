using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.6f;
    [SerializeField] private float _acceleration = 12f;

    private Transform _cameraTransform;

    private Rigidbody _rb;

    private Player_Controls _playerControls;

    private Vector2 _inputVector;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _cameraTransform = Camera.main.transform;
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
        _inputVector = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 cameraForward = _cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
        Vector2 input = Vector2.ClampMagnitude(_inputVector, 1f);
        Vector3 moveDirection = (cameraRight * input.x) + (cameraForward * input.y);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 targetVelocity = moveDirection * _moveSpeed;
        targetVelocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = targetVelocity;
    }
}
