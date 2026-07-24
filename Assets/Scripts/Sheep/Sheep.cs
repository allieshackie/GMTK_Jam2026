using Unity.VisualScripting;
using UnityEngine;

public enum SheepState
{
    Idle,
    Wander,
    Lure,
    Flee
};

public class Sheep : MonoBehaviour
{
    private SheepState _state;
    private FlockManager _flockManager;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _turnSpeed = 2f;

    [Tooltip("The amount of influence needed for a sheep to be attracted by a lure")]
    [SerializeField] private float _lureThreshold = 2f;
    private Vector3 _currentTarget;

    // Wander 
    [SerializeField] private float _wanderRadius = 2;
    private bool _waitingForNewTarget = true;

    // Animations
    private Animator _sheepAnimator;

    public void Init(FlockManager flockManager)
    {
        _flockManager = flockManager;
    }

    void Awake()
    {
    }

    void Start()
    {
        _sheepAnimator = GetComponent<Animator>();
        _state = SheepState.Idle;
    }

    public void SetState(SheepState newState)
    {
        _state = newState;

        if (_state == SheepState.Wander)
        {
            GetWanderTarget();
        }
    }

    void Idle()
    {
        
    }

    void Wander()
    {
        if (_waitingForNewTarget)
        {
            return;
        }

        Vector3 distanceFromTarget = _currentTarget - transform.position;
        if (distanceFromTarget.magnitude <= 0.1f)
        {
            // Reached wander target, pick another
            _waitingForNewTarget = true;
            _sheepAnimator.SetBool("IsMoving", false);
            Invoke(nameof(GetWanderTarget), Random.Range(1,5));
            return;
        }

        Vector3 direction = distanceFromTarget.normalized;
        transform.position += direction * _moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * _turnSpeed);
    }

    void CheckLureInfluence()
    {
        foreach(Lure lure in _flockManager.GetCurrentLures())
        {
            Vector3 distanceFromTarget = lure.transform.position - transform.position;
            float influence = lure.LureStrength * (1 - distanceFromTarget.magnitude / lure.Radius);
            Debug.Log($"Distance from lure: {distanceFromTarget.magnitude}");
            Debug.Log($"Influence: {influence}");
            if (influence >= _lureThreshold)
            {
                _currentTarget = lure.transform.position;
                SetState(SheepState.Lure);
                break;
            } 
        } 
    }

    void Lure()
    {
        Vector3 distanceFromTarget = _currentTarget - transform.position;
        if (distanceFromTarget.magnitude <= 0.1f)
        {
            _sheepAnimator.SetBool("IsMoving", false);
            return;
        }

        Vector3 direction = distanceFromTarget.normalized;
        transform.position += direction * _moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * _turnSpeed);
    }

    void Update()
    {
        switch(_state)
        {
            case SheepState.Idle: 
                break;
            case SheepState.Wander: 
                Wander();
                break;
            case SheepState.Lure:
                Lure();
                break;
            default:
                break;
        }

        if (_state != SheepState.Lure)
        {
            CheckLureInfluence();
        }
    }

    private void GetWanderTarget()
    {
        _waitingForNewTarget = false;
        _sheepAnimator.SetBool("IsMoving", true);
        Vector3 currentHomingTarget = _flockManager.GetTargetPoint();
        Vector2 randomOffset = Random.insideUnitCircle * _wanderRadius;
        _currentTarget = currentHomingTarget + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }
}
