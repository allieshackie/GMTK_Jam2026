using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wrymm : MonoBehaviour
{
    private enum WrymmState
    {
        Hunting,
        AttackingPlayer,
        AttackingSheep,
        //AttackingFence,
        Retreating
    }

    private enum TargetType
    {
        None,
        Player,
        Sheep,
        //Fence
    }

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float _moveSpeedDivider = 0.5f;
    [SerializeField] private float _retreatSpeed = 3f;
    [SerializeField] private float _retreatSpeedMultiplier = 3f;
    [SerializeField] private float _timeBeforeDestruction = 8f; // seconds

    [Header("Attacking")]
    [SerializeField] private float _attackDuration = 2f; // TODO: Make this match the length of the Attack Animation
    [SerializeField] private float _playerTargetCooldown = 2f;
    [SerializeField] private float _retreatTime = 3f;
    [SerializeField] private Animator _wyrmmAnimator;

    private FlockManager _flockManager;
    private Player _player;

    //private List<Fence> _fences;

    private Sheep _targetSheep;
    //private Fence _targetFence;

    private TargetType _targetType;
    private WrymmState _currentState;
    private Enemy _enemy;

    private bool _hasRetreated = false;
    private bool _ignorePlayer = false;
    private bool _fenceIsTarget = false;
    private bool _inLight = false;

    private Vector3 _retreatStartPosition;

    private void OnEnable()
    {
        //_fences = new List<Fence>(FindObjectsByType<Fence>());
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

            //case WrymmState.AttackingFence:
            //    break;

            case WrymmState.Retreating:
                Retreat();
                break;
        }
    }

    private void OnHit()
    {
        _wyrmmAnimator.SetTrigger("IsAttacked");
        RuntimeManager.PlayOneShotAttached("event:/Wyrms/wyrm_hit", gameObject);

        if (_targetSheep != null)
        {
            _targetSheep.Release();
            _targetSheep = null;
        }

        _currentState = WrymmState.Retreating;
    }

    private void Hunt()
    {
        if (_targetType == TargetType.Player && _player != null)
        {
            MoveTowards(_player.transform);
        }
        else if (_targetType == TargetType.Sheep && _targetSheep != null)
        {
            MoveTowards(_targetSheep.transform);
        }
        //else if (_targetType == TargetType.Fence && _targetFence != null && _targetFence.gameObject.activeInHierarchy)
        //{
        //    MoveTowards(_targetFence.transform);
        //}
        else
        {
            FindNewTarget();
        }
    }

    private void MoveTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float moveSpeed = _inLight ? _moveSpeed * _moveSpeedDivider : _moveSpeed;
        print($"moveSpeed: {moveSpeed}");

        _wyrmmAnimator.SetFloat("Velocity", moveSpeed);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        transform.position += moveSpeed * Time.deltaTime * direction;
    }

    private void Retreat()
    {
        _wyrmmAnimator.SetFloat("Velocity", _retreatSpeed);
        transform.position -= _retreatSpeed * Time.deltaTime * transform.forward;

        _retreatTime -= Time.deltaTime;
        if (_retreatTime < 0f && !_hasRetreated)
        {
            _hasRetreated = true;
            _retreatSpeed *= _retreatSpeedMultiplier;
        }

        _timeBeforeDestruction -= Time.deltaTime;
        if (_retreatTime < 0f)
        {
            if (_targetSheep != null)
            {
                _targetSheep.Kill();
            }

            Destroy(gameObject);
        }

    }

    private void FindNewTarget()
    {
        // Player is always considered first if they are allowed to be targeted and are closer than the other targets.
        if (_player != null && !_ignorePlayer)
        {
            Sheep closestSheep = GetClosestSheep();
            //Fence closestFence = GetClosestFence();

            float closestSheepDistanceSqr = Mathf.Infinity;
            float closestFenceDistanceSqr = Mathf.Infinity;
            float playerDistanceSqr =
                (_player.transform.position - transform.position).sqrMagnitude;

            if (closestSheep != null)
            {
                closestSheepDistanceSqr =
                    (closestSheep.transform.position - transform.position).sqrMagnitude;
            }

            //if (closestFence != null)
            //{
            //    closestFenceDistanceSqr = (closestFence.transform.position - transform.position).sqrMagnitude;
            //}

            float closestNonPlayerDistanceSqr =
                Mathf.Min(closestSheepDistanceSqr, closestFenceDistanceSqr);

            if (playerDistanceSqr < closestNonPlayerDistanceSqr)
            {
                _targetSheep = null;
                //_targetFence = null;
                _targetType = TargetType.Player;
                _currentState = WrymmState.Hunting;

                return;
            }
        }

        // Player wasn't selected. Randomly select between Sheep and Fence.
        //bool targetSheep = Random.value < 0.5f;

        //if (targetSheep)
        //{
        _targetSheep = GetClosestSheep();

        if (_targetSheep != null)
        {
            //_targetFence = null;
            _targetType = TargetType.Sheep;
            _currentState = WrymmState.Hunting;

            return;
        }
        //}
        //else
        //{
        //    _targetFence = GetClosestFence();

        //    if (_targetFence != null)
        //    {
        //        _targetSheep = null;
        //        _targetType = TargetType.Fence;
        //        _currentState = WrymmState.Hunting;

        //        return;
        //    }
        //}

        // If the randomly selected target type doesn't exist,
        // try the other target type.
        if (_targetSheep == null)
        {
            _targetSheep = GetClosestSheep();

            if (_targetSheep != null)
            {
                //_targetFence = null;
                _targetType = TargetType.Sheep;
                _currentState = WrymmState.Hunting;

                return;
            }
        }

        //if (_targetFence == null)
        //{
        //    _targetFence = GetClosestFence();

        //    if (_targetFence != null)
        //    {
        //        _targetSheep = null;
        //        _targetType = TargetType.Fence;
        //        _currentState = WrymmState.Hunting;

        //        return;
        //    }
        //}

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

    public void SetInLight(bool inLight)
    {
        _inLight = inLight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Anchor>() != null)
        {
            _inLight = true;
            print("Entered Light");
        }

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

        //// Check for a fence
        //Fence fence = other.GetComponent<Fence>();

        //if (fence != null)
        //{
        //    StartAttackingFence(fence);
        //}
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

        while (attackTimer < _attackDuration)
        {
            attackTimer += Time.deltaTime;
            yield return null;
        }

        _player.StartStun();
        RuntimeManager.PlayOneShotAttached("event:/Player/player_hit", gameObject);

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
            yield return null;
        }

        // Attack finished, grab the sheep
        GrabSheep();
        RuntimeManager.PlayOneShotAttached("event:/Wyrms/wyrm_grab", gameObject);

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

    //private Fence GetClosestFence()
    //{
    //    Fence closestFence = null;
    //    float closestDistanceSqr = Mathf.Infinity;

    //    foreach (Fence fence in _fences)
    //    {
    //        if (fence == null || !fence.gameObject.activeInHierarchy)
    //            continue;

    //        float distanceSqr =
    //            (fence.transform.position - transform.position).sqrMagnitude;

    //        if (distanceSqr < closestDistanceSqr)
    //        {
    //            closestDistanceSqr = distanceSqr;
    //            closestFence = fence;
    //        }
    //    }

    //    return closestFence;
    //}

    //private void StartAttackingFence(Fence fence)
    //{
    //    // Don't interrupt another attack
    //    if (_currentState != WrymmState.Hunting)
    //        return;

    //    _targetFence = fence;

    //    _fenceIsTarget = _targetType == TargetType.Fence;

    //    _currentState = WrymmState.AttackingFence;

    //    StartCoroutine(AttackFence());
    //}

    //private IEnumerator AttackFence()
    //{
    //    float attackTimer = 0f;

    //    _wyrmmAnimator.SetTrigger("IsAttacking");

    //    while (attackTimer < _attackDuration)
    //    {
    //        attackTimer += Time.deltaTime;

    //        // TODO: Add attack animation here


    //        yield return null;
    //    }

    //    if (_targetFence != null)
    //    {
    //        _targetFence.Damage();
    //    }
    //    _targetFence = null;

    //    // Fence was the actual target. Find a completely new target.
    //    if (_fenceIsTarget)
    //    {
    //        _fenceIsTarget = false;

    //        _targetSheep = null;
    //        _targetType = TargetType.None;

    //        FindNewTarget();
    //    }
    //    else
    //    {
    //        // Fence was blocking our path to a sheep. Continue targeting the same sheep.
    //        _targetType = TargetType.Sheep;
    //        _currentState = WrymmState.Hunting;
    //    }

    //    StopCoroutine(AttackFence());
    //}
}