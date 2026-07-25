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


    private FlockManager _flockManager;
    private Player _player;

    private Sheep _targetSheep;
    private Fence _targetFence;

    private TargetType _targetType;
    private WrymmState _currentState;

    private bool _ignorePlayer = false;

    private Vector3 _retreatStartPosition;

    private void OnEnable()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
        _player = FindAnyObjectByType<Player>();

        FindNewTarget();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case WrymmState.Hunting:
                Hunt();
                break;

            case WrymmState.AttackingPlayer:
                break;

            case WrymmState.AttackingSheep:
                break;

            case WrymmState.AttackingFence:
                break;

            case WrymmState.Retreating:
                Retreat();
                break;
        }
    }

    private void Hunt()
    {
        // Continuously check whether another target
        // has become closer than the current target.
        FindNewTarget();

        if (_targetType == TargetType.Player && _player != null)
        {
            MoveTowards(_player.transform);
        }
        else if (_targetType == TargetType.Sheep && _targetSheep != null)
        {
            MoveTowards(_targetSheep.transform);
        }
        else
        {
            FindNewTarget();
        }
    }

    private void MoveTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += _moveSpeed * Time.deltaTime * direction;
    }

    private void Retreat()
    {
        transform.position -= _retreatSpeed * Time.deltaTime * transform.forward;
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
            StartAttackingFence(fence);
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