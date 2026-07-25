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

    private AnglerState _state;

    private Vector3 _velocity;

    private FlockManager _flockManager;

    private Player _player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
        _player = FindAnyObjectByType<Player>();
        SetState(AnglerState.Hunt);
    }

    
    public void SetState(AnglerState newState)
    {
        _state = newState;
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

        float strength = 1f - (distance / _avoidDistance);
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

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.01f)
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
        float playerDistance = Vector3.Distance(position, _player.transform.position) * _avoidPlayerForce;
        rank += Mathf.Clamp(playerDistance, 0, _minPlayerDistance);
        return rank;
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
        if (_findNewHuntingSpot)
        {
            _findNewHuntingSpot = false;
            _huntingSpot = FindHuntingSpot();
        }

        Vector3 distanceFromTarget = _huntingSpot - transform.position;
        float distance = distanceFromTarget.magnitude;

        if (distance <= 0.5f)
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

    void Lure()
    {
        if (_currentLure == null)
        {
            _currentLure = Instantiate(_lurePrefab, transform.position, Quaternion.identity);
            _flockManager.GetClosestSheep(transform.position, out float lureRadius);
            lureRadius = Math.Max(lureRadius + 3, 15f);
            _currentLure.Initialize(lureRadius, 12f);
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
                Lure();
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
