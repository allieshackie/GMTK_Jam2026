using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.6f;
    [SerializeField] private Transform _playerMesh;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private Collider _attackCollider;

    private Player_Controls _playerControls;
    private Transform _cameraTransform;
    private Rigidbody _rb;
    private Vector2 _inputVector;

    #region Input Subscription
    private void SubscribeInputs()
    {
        _playerControls.Player.Movement.performed += OnMove;
        _playerControls.Player.Movement.canceled += OnMove;

        _playerControls.Player.Attack.performed += OnAttack;
        _playerControls.Player.Attack.canceled += OnAttack;

        _playerControls.Player.BellLure.performed += OnBellLure;
        _playerControls.Player.BellLure.canceled += OnBellLure;

        _playerControls.Player.Disable();
    }

    private void UnsubscribeInputs()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

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
        //GameManager.Instance.OnGameStateChanged += EnableControls;
    }
    private void OnDisable()
    {
        UnsubscribeInputs();
    }

    private void EnableControls(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelStart)
        {   
            SubscribeInputs();
        }
        else if (state == GameManager.GameState.GameWon || state == GameManager.GameState.GameLost)
        {
            UnsubscribeInputs();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _inputVector = context.action.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
    }

    private void OnBellLure(InputAction.CallbackContext context)
    {
    }

    void Start()
    {
        _playerControls.Player.Enable();
    }

    private void Update()
    {
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

        if (moveDirection != Vector3.zero)
        {
            _playerMesh.transform.rotation = Quaternion.Slerp(_playerMesh.transform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 10f);
        }

        Vector3 targetVelocity = moveDirection * _moveSpeed;
        targetVelocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = targetVelocity;

        _playerAnimator.SetFloat("Velocity", _rb.linearVelocity.magnitude);
    }
}