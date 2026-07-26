using Unity.VisualScripting;
using UnityEngine;

public enum SheepState
{
    Idle,
    Wander,
    Lure,
    Grabbed,
    Flee
};

public class Sheep : MonoBehaviour
{
    
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _acceleration = 2.0f;
    [SerializeField] private float _turnSpeed = 2f;

    [Tooltip("The amount of influence needed for a sheep to be attracted by a lure")]
    [SerializeField] private float _lureThreshold = 0;
    [SerializeField] private float _homeInfluence = 50f;
    [SerializeField] private float _returnHomeDistance = 2f;
    // Wander 
    [SerializeField] private float _wanderRadius = 2;
    [SerializeField] private float _wanderForce = 0.3f;
     [SerializeField] private float _wanderSpeed = 2f;
    private bool _waitingForNewTarget = true;
    
    // Separation
    [SerializeField] private float _separationRadius = 1.5f;
    [SerializeField] private float _separationForce = 2.0f;

    // Obstacle
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _avoidDistance = 2.0f;
    [SerializeField] private float _avoidRadius = 0.4f;
    [SerializeField] private float _avoidForce = 2.0f;

    // Ground Calcs
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundRayDistance = 5f;
    [SerializeField] private float _groundOffset = 0.05f;

    // Flee 
    [SerializeField] private float _panicRadius = 8f;
    [SerializeField] private float _fleeForce = 2.0f;
    [SerializeField] private float _fleeTime = 2.0f;
    [SerializeField] private float _fleeSpeed = 2.0f;
    private Vector3 _fleeDirection;
    private float _fleeTimer;

    private Vector3 _currentTarget;
    private Vector3 _velocity;
    private SheepState _state;
    private FlockManager _flockManager;

    // Animations
    [SerializeField] private Animator _sheepAnimator;

    private float _randomLogicOffset;

    private bool _gameStart = false;

    private Rigidbody _rigidbody;

    public void Init(FlockManager flockManager)
    {
        _flockManager = flockManager;
    }

    void Start()
    {
        SetState(SheepState.Idle);
        GameManager.Instance.OnGameStateChanged += CheckGameState;
        _currentTarget = _flockManager.GetHerdHomePoint();
        _randomLogicOffset = EntityId.ToULong(GetEntityId()) % 10 * 0.5f;

        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= CheckGameState;
    }

    private void CheckGameState(GameManager.GameState state)
    {
        _gameStart = (state == GameManager.GameState.LevelStart) || (state == GameManager.GameState.Playing);
    }

    public void SetState(SheepState newState)
    {
        _state = newState;

        if (_state == SheepState.Wander)
        {
            float delay = Random.Range(6f, 25f) + _randomLogicOffset;
            Invoke(nameof(GetWanderTarget), delay);
        }

        if (_state == SheepState.Flee)
        {
            Angler angler = GetClosestAngler();
            if (angler)
            {
                Vector3 away = transform.position - angler.transform.position;
                Vector3 random = Random.insideUnitSphere;
                random.y = 0;
                _fleeDirection = (away.normalized + random * 0.5f).normalized;
                _fleeTimer = _fleeTime;
            }
        }
    }

    public SheepState GetState()
    {
        return _state;
    }

    public void Grab(Transform monsterTransform)
    {
        transform.SetParent(monsterTransform);
        SetState(SheepState.Grabbed);
    }

    public void Release()
    {
        transform.SetParent(null);
        SetState(SheepState.Flee);
    }

    public void Kill()
    {
        _flockManager.RemoveSheep(this);
        Destroy(gameObject);
    }

    private Vector3 Separation()
    {
        Vector3 separation = Vector3.zero;
        foreach (Sheep other in _flockManager.GetCurrentFlock())
        {
            if (other == this)
            {
                continue;
            }

            Vector3 distanceFromSheep = transform.position - other.transform.position;
            float distance = distanceFromSheep.magnitude;
            if (distance < _separationRadius)
            {
                separation += distanceFromSheep.normalized / distance;
            }
        }

        return separation;
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

    private void Idle()
    {
        if (!_gameStart)
        {
            return;
        }
        if (_currentTarget == _flockManager.GetHerdHomePoint())
        {   
            if (Vector3.Distance(_currentTarget, transform.position) <= _wanderRadius)
            {
                // Queue wander after some time
                SetState(SheepState.Wander);
            }
        }
    }

    private Vector3 Wander()
    {
        if (_waitingForNewTarget)
        {
            return Vector3.zero;
        }

        Vector3 distanceFromTarget = _currentTarget - transform.position;
        float distance = distanceFromTarget.magnitude;
        if (distance <= 1.5f)
        {
            // Reached wander target, pick another
            _waitingForNewTarget = true;
            float delay = Random.Range(6f, 25f) + _randomLogicOffset;
            Invoke(nameof(GetWanderTarget), delay);
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
    private void GetWanderTarget()
    {
        _waitingForNewTarget = false;
        Vector3 currentHomingTarget = _flockManager.GetHerdHomePoint();
        Vector2 randomOffset = Random.insideUnitCircle * _wanderRadius;
        _currentTarget = currentHomingTarget + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }

    private void CheckLureInfluence()
    {
        Lure strongestLure = null;
        float strongestInfluence = 0f;

        foreach (Lure lure in _flockManager.GetCurrentLures())
        {
            Vector3 offset = lure.transform.position - transform.position;
            float distance = offset.magnitude;

            if (distance > lure.Radius)
            {
                continue;
            }
            float influence = lure.LureStrength * (1 - offset.magnitude / lure.Radius);

            if (influence > strongestInfluence)
            {
                strongestInfluence = influence;
                strongestLure = lure;
            }
        }

        // If have a herd home point, and the home influence is stronger than the strongest lure, return home
        if ((strongestLure == null || strongestInfluence < _homeInfluence) && _flockManager.IsHomePointSet() && _currentTarget != _flockManager.GetHerdHomePoint())
        {
            if (Vector3.Distance(transform.position, _flockManager.GetHerdHomePoint()) <= _returnHomeDistance)
            {
                _currentTarget = _flockManager.GetHerdHomePoint();
                SetState(SheepState.Idle);
                return;
            }
        }

        if (strongestLure != null && strongestInfluence >= _lureThreshold)
        {
            _currentTarget = strongestLure.transform.position;
            SetState(SheepState.Lure);
        }
    }

    private Vector3 Lure()
    {
        Vector3 distanceFromTarget = _currentTarget - transform.position;
        if (distanceFromTarget.magnitude <= 0.2f)
        {
            _velocity = Vector3.zero;
            return Vector3.zero;
        }

        return distanceFromTarget.normalized;
    }

    private void Move(Vector3 sheepSteering)
    {
        sheepSteering.y = 0;

        float speed = _moveSpeed;
        if (_state == SheepState.Wander)
        {
            speed = _wanderSpeed;
        }
        else if (_state == SheepState.Flee)
        {
            speed = _fleeSpeed;
        }

        Vector3 desiredVelocity = sheepSteering.normalized * speed;
        _velocity = Vector3.Lerp(_velocity, sheepSteering.normalized * speed, Time.deltaTime * _acceleration);

        if (desiredVelocity.sqrMagnitude < 0.001f && _velocity.sqrMagnitude < 0.03f)
        {
            _velocity = Vector3.zero;
            return;
        }

        transform.position += _velocity * Time.deltaTime;
        //_rigidbody.MovePosition(transform.position + _velocity * Time.deltaTime);

        if (_velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = _velocity.normalized;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * _turnSpeed);
        }

        _sheepAnimator.SetFloat("Velocity", _velocity.magnitude);
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

    private void CheckForPanicState()
    {
        if (_state == SheepState.Flee)
        {
            return;
        }
        foreach (Sheep sheep in _flockManager.GetCurrentFlock())
        {
            if (sheep == this)
            {
                continue;
            }

            if (sheep.GetState() == SheepState.Grabbed)
            {
                float distance = Vector3.Distance(transform.position, sheep.transform.position);
                if (distance <= _panicRadius)
                {
                    SetState(SheepState.Flee);
                    return;
                }
            }
        }
    }
    public Angler GetClosestAngler()
    {
        Angler[] allAnglers = FindObjectsByType<Angler>();
        Angler closest = null;
        float distance = float.MaxValue;

        foreach (Angler angler in allAnglers)
        {
            float currentDistance = Vector3.Distance(transform.position, angler.transform.position);
            if (currentDistance < distance)
            {
                closest = angler;
            }
        }

        return closest;
    }

    private Vector3 Flee()
    {
        return _fleeDirection;
    }

    private void CountdownFleeState()
    {
        _fleeTimer -= Time.deltaTime;
        if (_fleeTimer <= 0)
        {
            SetState(SheepState.Idle);
        }
    }

    void Update()
    {
        Vector3 sheepSteer = Vector3.zero;

        switch(_state)
        {
            case SheepState.Idle: 
                Idle();
                break;
            case SheepState.Wander: 
                sheepSteer += Wander() * _wanderForce;
                break;
            case SheepState.Lure:
                sheepSteer += Lure();
                break;
            case SheepState.Grabbed:
                // Disable all movement
                return;
            case SheepState.Flee:
                sheepSteer += Flee() * _fleeForce;
                CountdownFleeState();
                break;
            default:
                break;
        }

        sheepSteer += Separation() * _separationForce;
        sheepSteer += AvoidObstacle() * _avoidForce;

        Move(sheepSteer);
        UpdateGroundPosition();

        CheckLureInfluence();
        CheckForPanicState();
    }
}
