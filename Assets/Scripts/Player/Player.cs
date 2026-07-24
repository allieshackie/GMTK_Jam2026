using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.6f;
    [SerializeField] private float _acceleration = 12f;

    [SerializeField] private float _sprintModifier = 1.75f;

    [SerializeField] private Lure _lurePrefab;

    [SerializeField] private float _attackCooldown = 0.05f;

    [SerializeField] private float _lureCooldown = 0.05f;

    private bool _isAttacking = false;
    private bool _canAttack = true;
    private float _currentAttackCooldown = 0f;

    private bool _isSprinting = false;

    private bool _canLure = false;

     private float _currentLureCooldown = 0f;

    private Player_Controls _playerControls;
    private Transform _cameraTransform;
    private Rigidbody _rb;
    private Vector2 _inputVector;

    #region Input Subscription
    private void SubscribeInputs() 
    {
        _playerControls.Player.Movement.performed += OnMove;
        _playerControls.Player.Movement.canceled += OnMove;

        _playerControls.Player.Sprint.performed += OnSprint;
        _playerControls.Player.Sprint.canceled += OnSprint;

        _playerControls.Player.Attack.performed += OnAttack;
        _playerControls.Player.Attack.canceled += OnAttack;

        _playerControls.Player.BellLure.performed += OnBellLure;
        _playerControls.Player.BellLure.canceled += OnBellLure;

        _playerControls.Player.Enable();
    }

    private void UnsubscribeInputs()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Sprint.performed -= OnSprint;
        _playerControls.Player.Sprint.canceled -= OnSprint;

        _playerControls.Player.Attack.performed -= OnAttack;
        _playerControls.Player.Attack.canceled -= OnAttack;

        _playerControls.Player.BellLure.performed -= OnBellLure;
        _playerControls.Player.BellLure.canceled -= OnBellLure;

        _playerControls.Player.Disable();
    }
    #endregion

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();

        SubscribeInputs();

    }

    private void OnDisable()
    {
        UnsubscribeInputs();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _inputVector = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValueAsButton();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        _isAttacking = context.ReadValueAsButton();
        print("Player Attack");
    }

    private void OnBellLure(InputAction.CallbackContext context)
    {
        bool isLuring = context.ReadValueAsButton();
        if(isLuring && _canLure)
        {
            _currentLureCooldown = _lureCooldown;
            _canLure = false;
            Lure _bellLure = Instantiate(_lurePrefab, transform.position, Quaternion.identity);
            _bellLure.Initialize(40f, 15f);

            Destroy(_bellLure.gameObject, 3f);
        }
    }

    private void Update()
    {
        if (_isAttacking && _canAttack)
        {
            _currentAttackCooldown = _attackCooldown;
            _canAttack = false;
        
            // Play Attack Anim
        }

        if (!_canAttack)
        {
            _currentAttackCooldown -= Time.deltaTime;
            _canAttack = _currentAttackCooldown <= 0f ? true : false;
        }

        if (!_canLure)
        {
            _currentLureCooldown -= Time.deltaTime;
            _canLure = _currentLureCooldown <= 0f ? true : false;
        }
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

        float currentMoveSpeed = _isSprinting ? _moveSpeed * _sprintModifier : _moveSpeed;
        Vector3 targetVelocity = moveDirection * currentMoveSpeed;
        targetVelocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = targetVelocity;
    }
}
