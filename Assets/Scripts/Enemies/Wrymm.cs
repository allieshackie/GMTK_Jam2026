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
    [SerializeField] private Animator _wyrmmAnimator;

    private FlockManager _flockManager;
    private Player _player;

    private Sheep _targetSheep;
    private Fence _targetFence;

    private TargetType _targetType;
    private WrymmState _currentState;
    private Enemy _enemy;

    private bool _ignorePlayer = false;

    private Vector3 _retreatStartPosition;

    private void OnEnable()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
        _player = FindAnyObjectByType<Player>();
        _enemy = GetComponent<Enemy>();

        _enemy.OnHit += OnHit;

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

    private void OnHit()
    {
        if (_targetSheep != null)
        {
            _targetSheep.Release();
            _targetSheep = null;

            _currentState = WrymmState.Retreating;
        }
    }

    private void Hunt()
    {
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

        _wyrmmAnimator.SetFloat("Velocity", _moveSpeed);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        transform.position += _moveSpeed * Time.deltaTime * direction;
    }

    private void Retreat()
    {
        _wyrmmAnimator.SetFloat("Velocity", _retreatSpeed);
        transform.position -= _retreatSpeed * Time.deltaTime * transform.forward;
    }

    private void FindNewTarget()
    {
        Sheep closestSheep = GetClosestSheep();
        float closestSheepDistanceSqr = Mathf.Infinity;
        float playerDistanceSqr = Mathf.Infinity;

        if (closestSheep != null)
        {
            closestSheepDistanceSqr = (closestSheep.transform.position - transform.position).sqrMagnitude;
        }

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

        _wyrmmAnimator.SetTrigger("IsAttacking"); // Moved it out of th while loop, as the attack anim is only a trigger, feel free to move this noah, this just fires a single request to attack to the animation controller.
        //_wyrmmAnimator.SetTrigger("IsAttacked"); // Not sure where the on wyrmm hit stuff is, so feel free to move this where that exists.
        
        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO: Add attack animation here


            yield return null;
        }
        // TODO: Stun player

        _ignorePlayer = true;

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

        _wyrmmAnimator.SetTrigger("IsAttacking");
        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO: Add attack animation here

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
        _targetSheep.Grab(transform);
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

        _wyrmmAnimator.SetTrigger("IsAttacking");

        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;

            // TODO: Add attack animation here


            yield return null;
        }

        if (_targetFence != null)
        {
            _targetFence.Damage();
        }
        _targetFence = null;

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