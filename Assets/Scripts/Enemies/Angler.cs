using System;
using UnityEngine;

public enum AnglerState
{
    Hunt,
    Lure,
    Grab,
    Stun,
    Flee
};

public class Angler : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _acceleration = 2.0f;
    [SerializeField] private float _turnSpeed = 2f;

    // Obstacle
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _avoidDistance = 2.0f;
    [SerializeField] private float _avoidRadius = 0.4f;
    [SerializeField] private float _avoidForce = 2.0f;

    [SerializeField] private float _avoidPlayerForce = 2.0f;

    // Ground Calcs
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundRayDistance = 5f;
    [SerializeField] private float _groundOffset = 0.05f;

    // Hunting 
    [SerializeField] private float _preferredFlockDistance = 10f;
    [SerializeField] private float _huntRadius = 10f;
    [SerializeField] private float _minPlayerDistance = 3f;

    private Vector3 _huntingSpot;

    private bool _findNewHuntingSpot = true;

    // Lure
    [SerializeField] private Lure _lurePrefab;

    private Lure _currentLure;

    // Grabbing
    [SerializeField] private float _grabbingDistance;
    [SerializeField] private float _sheepCapturedTime;
    [SerializeField] private float _capturedSheepMoveSpeed = 1f;

    //Animation
    [SerializeField] private Animator _anglerAnimator;

    [SerializeField] private float _stunTime = 2f;

    // Flee
    [SerializeField] private float _fleeTime = 2.0f;
    [SerializeField] private float _fleeSpeed = 2.0f;
    private Vector3 _fleeDirection;
    private float _fleeTimer;

    private Sheep _grabbedSheep;
    private float _captureTimer;

    private float _stunnedTimer;

    private AnglerState _state;

    private Vector3 _velocity;

    private FlockManager _flockManager;

    private Player _player;

    private Enemy _enemy; // self

    void Start()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
        _player = FindAnyObjectByType<Player>();
        _enemy = GetComponent<Enemy>();
        _enemy.OnHit += OnHit;
        SetState(AnglerState.Hunt);
    }

    
    public void SetState(AnglerState newState)
    {
        _state = newState;

        if (_state == AnglerState.Hunt)
        {
            _findNewHuntingSpot = true;
        }

        if (_state == AnglerState.Stun)
        {
            _stunnedTimer = _stunTime;
        }

        if (_state == AnglerState.Flee)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player)
            {
                Vector3 away = transform.position - player.transform.position;
                Vector3 random = UnityEngine.Random.insideUnitSphere;
                random.y = 0;
                _fleeDirection = (away.normalized + random * 0.5f).normalized;
                _fleeTimer = _fleeTime;
            }
        }
    }

    private Vector3 AvoidObstacle()
    {
        Vector3 direction = _velocity.sqrMagnitude > 0.01f ? _velocity.normalized : transform.forward;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, direction);

        if (Physics.SphereCast(ray, _avoidRadius, out RaycastHit hit, _avoidDistance, _obstacleLayer))
        {
            return Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
        }

        return Vector3.zero;
    }

    private Vector3 AvoidPlayer()
    {
        Vector3 toPlayer = _player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > _minPlayerDistance)
        {
            return Vector3.zero;
        }

        Vector3 away = -toPlayer.normalized;

        float strength = 1f - (distance / _minPlayerDistance);

        return away * strength;
    }

    private void Move(Vector3 steer)
    {
        steer.y = 0;

        _velocity = Vector3.Lerp(_velocity, steer, Time.deltaTime * _acceleration);
        if (steer.sqrMagnitude < 0.001f && _velocity.sqrMagnitude < 0.03f)
        {
            _velocity = Vector3.zero;
            return;
        }

        _anglerAnimator.SetFloat("Velocity", _velocity.sqrMagnitude);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.01f && _state != AnglerState.Lure)
        {
            Vector3 direction = _velocity.normalized;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * _turnSpeed);
        }
    }

    private void UpdateGroundPosition()
    {
        Ray rayToGround = new Ray(transform.position + Vector3.up, Vector3.down);

        if (Physics.Raycast(rayToGround, out RaycastHit hit, _groundRayDistance, _groundLayer))
        {
            Vector3 position = transform.position;
            position.y = hit.point.y + _groundOffset;

            transform.position = position;
        }
    }

    private float RankSpotPlacement(Vector3 position)
    {
        float rank = 0;

        float herdDistance = Vector3.Distance(position, _flockManager.GetHerdHomePoint());
        rank -= Mathf.Abs(herdDistance - _preferredFlockDistance);

        // Prefer being farther from player
        float playerDistance = Vector3.Distance(position, _player.transform.position);
        rank += Mathf.Clamp(playerDistance * _avoidPlayerForce, 0, 20);
        return rank;
    }

    private bool IsHuntingSpotValid()
    {
        float playerDistance = Vector3.Distance(_huntingSpot, _player.transform.position);
        return playerDistance >= _minPlayerDistance;
    }

    private Vector3 FindHuntingSpot()
    {
        Vector3 herdHome = _flockManager.GetHerdHomePoint();

        Vector3 bestSpot = transform.position;
        float bestScore = float.MinValue;

        for (int i = 0; i < 20; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * _huntRadius;
            Vector3 possibleSpot = herdHome + new Vector3(offset.x, 0, offset.y);

            float score = RankSpotPlacement(possibleSpot);
            if (score > bestScore)
            {
                bestScore = score;
                bestSpot = possibleSpot;
            }
        }

        return bestSpot;
    }

    Vector3 Hunting()
    {
        if (_findNewHuntingSpot || !IsHuntingSpotValid())
        {
            _findNewHuntingSpot = false;
            _huntingSpot = FindHuntingSpot();
        }

        Vector3 distanceFromTarget = _huntingSpot - transform.position;
        float distance = distanceFromTarget.magnitude;

        if (distance <= 1.5f)
        {
            _velocity = Vector3.zero;
            SetState(AnglerState.Lure);
            return Vector3.zero;
        }

        float speed = _moveSpeed;
        float slowRadius = 3f;
        if (distance < slowRadius)
        {
            speed *= distance / slowRadius;
        }

        return distanceFromTarget.normalized * speed;
    }

    private void FaceLuringDirection()
    {
        Vector3 lookDirection;
        Sheep closestSheep = _flockManager.GetClosestSheep(transform.position, out float distance);
        if (closestSheep != null && distance < _grabbingDistance)
        {
            lookDirection = closestSheep.transform.position - transform.position;
        }
        else
        {
            lookDirection = _flockManager.GetHerdHomePoint() - transform.position;
        }

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            lookDirection.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, lookDirection.normalized, Time.deltaTime * _turnSpeed);
        }
    }

    void Lure()
    {
        if (_currentLure == null)
        {
            _anglerAnimator.SetBool("IsLuring", true);
            _currentLure = Instantiate(_lurePrefab, transform.position, Quaternion.identity);
            _flockManager.GetClosestSheep(transform.position, out float lureRadius);
            lureRadius = Math.Max(lureRadius + 3, 15f);
            _currentLure.Initialize(lureRadius, 12f);
        }
    }

    private void GrabSheep(Sheep sheep)
    {
        _anglerAnimator.SetBool("IsLuring", false);
        _grabbedSheep = sheep;
        sheep.Grab(transform);

        _captureTimer = _sheepCapturedTime;
        SetState(AnglerState.Grab);
    }
    
    private void CheckForSheepToGrab()
    {
        if (_state != AnglerState.Grab)
        {
            Sheep closest = _flockManager.GetClosestSheep(transform.position, out float distance);
            if (closest != null && distance <= _grabbingDistance)
            {
                GrabSheep(closest);
                if (_currentLure)
                {
                    Destroy(_currentLure);
                }
            }
        }
    }

    private Vector3 CaptureSheep()
    {
        _captureTimer -= Time.deltaTime;
        if (_captureTimer <= 0)
        {
            _grabbedSheep.Kill();
            _grabbedSheep = null;
            SetState(AnglerState.Hunt);
        }
        return (transform.position - _flockManager.GetHerdHomePoint()).normalized * _capturedSheepMoveSpeed;
    }

    public void OnHit()
    {
        if (_grabbedSheep != null)
        {
            _grabbedSheep.Release();
            _grabbedSheep = null;

            SetState(AnglerState.Stun);
        }
        if (_currentLure != null)
        {
            Destroy(_currentLure);
        }
    }

    private void Stunned()
    {
        _stunnedTimer -= Time.deltaTime;
        if (_stunnedTimer <= 0)
        {
            SetState(AnglerState.Flee);
        }
    }

    private Vector3 Flee()
    {
        return _fleeDirection * _fleeSpeed;
    }

    private void CountdownFleeState()
    {
        _fleeTimer -= Time.deltaTime;
        if (_fleeTimer <= 0)
        {
            SetState(AnglerState.Hunt);
        }
    }

    void Update()
    {
        Vector3 steer = Vector3.zero;

        switch(_state)
        {
            case AnglerState.Hunt:
                steer += Hunting();
                break;
            case AnglerState.Lure:
                CheckForSheepToGrab();
                Lure();
                FaceLuringDirection();
                break;
            case AnglerState.Grab:
                steer += CaptureSheep();
                break;
            case AnglerState.Stun:
                Stunned();
                return;
            case AnglerState.Flee:
                steer += Flee();
                CountdownFleeState();
                break;
            default:
                break;
        }

        // Angler should stop moving when luring or actively grabbing sheep
        if (_state != AnglerState.Lure)
        {   
            steer += AvoidObstacle() * _avoidForce;
            steer += AvoidPlayer() * _avoidPlayerForce;
        }

        Move(steer);
        UpdateGroundPosition();
    }
}
