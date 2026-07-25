using System.Collections;
using UnityEngine;

public class Wrymm : MonoBehaviour
{
    private enum WrymmState
    {
        Hunting,
        AttackingPlayer,
        AttackingSheep,
        AttackingFence,
        Retreating
    }

    private enum TargetType
    {
        None,
        Player,
        Sheep
    }

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _retreatSpeed = 3f;

    [Header("Attacking")]
    [SerializeField] private float _attackDuration = 2f; // TODO: Make this match the length of the Attack Animation
    [SerializeField] private float _playerTargetCooldown = 2f;

    // Obstacle
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _avoidDistance = 2.0f;
    [SerializeField] private float _avoidRadius = 0.4f;
    [SerializeField] private float _avoidForce = 2.0f;

    private FlockManager _flockManager;
    private Player _player;

    private Sheep _targetSheep;
    private Fence _targetFence;

    private TargetType _targetType;
    private WrymmState _currentState;

    private bool _ignorePlayer = false;

    private Vector3 _retreatStartPosition;

    private Vector3 _velocity;

    private bool _goAroundObstacle = false;

    private void OnEnable()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
        _player = FindAnyObjectByType<Player>();

        FindNewTarget();
    }

    private void Update()
    {
        Vector3 steer = Vector3.zero;

        switch (_currentState)
        {
            case WrymmState.Hunting:
                steer += Hunt();
                break;

            case WrymmState.AttackingPlayer:
                break;

            case WrymmState.AttackingSheep:
                break;

            case WrymmState.AttackingFence:
                break;

            case WrymmState.Retreating:
                steer += Retreat();
                break;
        }

        if (_goAroundObstacle)
        {
            steer += AvoidObstacle() * _avoidForce;
            Invoke("CancelAvoid", 0.5f);
        }

        MoveTowards(steer);
    }

    private Vector3 Hunt()
    {
        // Continuously check whether another target
        // has become closer than the current target.
        FindNewTarget();

        if (_targetType == TargetType.Player && _player != null)
        {
            Vector3 distanceFromTarget = _player.transform.position - transform.position;
            return distanceFromTarget.normalized;
            //MoveTowards(_player.transform);
        }
        else if (_targetType == TargetType.Sheep && _targetSheep != null)
        {
            Vector3 distanceFromTarget = _targetSheep.transform.position - transform.position;
            return distanceFromTarget.normalized;
            //MoveTowards(_targetSheep.transform);
        }
        else
        {
            FindNewTarget();
        }

        return Vector3.zero;
    }

    private bool ShouldAttackObstacle()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;

        Vector3 left = Quaternion.Euler(0, -30, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 30, 0) * forward;

        bool centerBlocked = Physics.Raycast(origin, forward, _avoidDistance, _obstacleLayer);
        bool leftBlocked = Physics.Raycast(origin, left, _avoidDistance, _obstacleLayer);
        bool rightBlocked = Physics.Raycast(origin, right, _avoidDistance, _obstacleLayer);
        
        return centerBlocked && leftBlocked && rightBlocked;
    }

    private void CancelAvoid()
    {
        _goAroundObstacle = false;
    }

    private Vector3 AvoidObstacle()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;

        Vector3 left = Quaternion.Euler(0, -30, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 30, 0) * forward;

        bool centerBlocked = Physics.Raycast(origin, forward, _avoidDistance, _obstacleLayer);
        bool leftBlocked = Physics.Raycast(origin, left, _avoidDistance, _obstacleLayer);
        bool rightBlocked = Physics.Raycast(origin, right, _avoidDistance, _obstacleLayer);

        if (!centerBlocked)
        {
            return Vector3.zero;
        }

        if (!leftBlocked)
        {
            return left;
        }

        if (!rightBlocked)
        {
            return right;
        }

        // Fully blocked
        return Vector3.zero;
    }

    private void MoveTowards(Vector3 target)
    {
        target.y = 0;

        _velocity = Vector3.Lerp(_velocity, target, Time.deltaTime);
        if (target.sqrMagnitude < 0.001f && _velocity.sqrMagnitude < 0.03f)
        {
            _velocity = Vector3.zero;
            return;
        }

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = _velocity.normalized;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 0.5f);
        }
    }

    private Vector3 Retreat()
    {
        return (transform.position - transform.forward) * _retreatSpeed;
    }

    private void FindNewTarget()
    {
        Sheep closestSheep = GetClosestSheep();

        float closestSheepDistanceSqr = Mathf.Infinity;

        if (closestSheep != null)
        {
            closestSheepDistanceSqr = (closestSheep.transform.position - transform.position).sqrMagnitude;
        }

        float playerDistanceSqr = Mathf.Infinity;

        if (_player != null && !_ignorePlayer)
        {
            playerDistanceSqr = (_player.transform.position - transform.position).sqrMagnitude;
        }

        // Player is closer than the closest sheep
        if (_player != null && playerDistanceSqr < closestSheepDistanceSqr)
        {
            _targetSheep = null;
            _targetType = TargetType.Player;
            _currentState = WrymmState.Hunting;

            return;
        }

        // Sheep is closer than the player
        if (closestSheep != null)
        {
            _targetSheep = closestSheep;
            _targetType = TargetType.Sheep;
            _currentState = WrymmState.Hunting;

            return;
        }

        // No valid targets
        _targetSheep = null;
        _targetType = TargetType.None;
    }

    private Sheep GetClosestSheep()
    {
        Sheep closestSheep = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Sheep sheep in _flockManager.GetCurrentFlock())
        {
            if (sheep == null)
                continue;

            float distanceSqr = (sheep.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestSheep = sheep;
            }
        }

        return closestSheep;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for the player
        Player player = other.GetComponent<Player>();

        if (player != null && _targetType == TargetType.Player && player == _player)
        {
            StartAttackingPlayer();
            return;
        }

        // Check for the sheep
        Sheep sheep = other.GetComponent<Sheep>();

        if (sheep != null && _targetType == TargetType.Sheep && sheep == _targetSheep)
        {
            StartAttackingSheep();
            return;
        }

        // Check for a fence
        Fence fence = other.GetComponent<Fence>();

        if (fence != null)
        {
            Debug.Log("Entered Fence");
            if (ShouldAttackObstacle())
            {
                Debug.Log("Attack Fence");
                StartAttackingFence(fence);
            }
            else
            {
                _goAroundObstacle = true;
            }
        }
    }

    private void StartAttackingPlayer()
    {
        if (_currentState != WrymmState.Hunting)
            return;

        _currentState = WrymmState.AttackingPlayer;

        StartCoroutine(AttackPlayer());
    }

    private IEnumerator AttackPlayer()
    {
        float attackTimer = 0f;

        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO:
            // Add attack animation here

            yield return null;
        }
        // Stun player

        _ignorePlayer = true;

        // Attack finished.
        // Immediately find a sheep.
        FindNewSheepTarget();

        yield return new WaitForSeconds(_playerTargetCooldown);
        StopCoroutine(AttackPlayer());
    }

    private void StartAttackingSheep()
    {
        if (_currentState != WrymmState.Hunting)
            return;

        _currentState = WrymmState.AttackingSheep;

        StartCoroutine(AttackSheep());
    }

    private IEnumerator AttackSheep()
    {
        float attackTimer = 0f;

        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO:
            // Add attack animation here

            yield return null;
        }

        // Attack finished, grab the sheep
        GrabSheep();

        // Begin retreating
        _currentState = WrymmState.Retreating;
        StopCoroutine(AttackSheep());
    }

    private void GrabSheep()
    {
        if (_targetSheep == null)
            return;

        _retreatStartPosition = transform.position;

        _targetSheep.transform.SetParent(transform);
    }

    private void StartAttackingFence(Fence fence)
    {
        // Don't interrupt another attack
        if (_currentState != WrymmState.Hunting)
            return;

        _targetFence = fence;

        _currentState = WrymmState.AttackingFence;

        StartCoroutine(AttackFence());
    }

    private IEnumerator AttackFence()
    {
        float attackTimer = 0f;

        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO:
            // Add attack animation here


            yield return null;
        }

        if (_targetFence != null)
        {
            _targetFence.Damage();
        }
        _targetFence = null;

        // Find a new target after destroying/attacking fence
        FindNewTarget();

        StopCoroutine(AttackFence());
    }

    private void FindNewSheepTarget()
    {
        _targetSheep = GetClosestSheep();

        if (_targetSheep != null)
        {
            _targetType = TargetType.Sheep;
            _currentState = WrymmState.Hunting;
        }
        else
        {
            _targetType = TargetType.None;
        }
    }
}