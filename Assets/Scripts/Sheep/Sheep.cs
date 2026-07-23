using Unity.VisualScripting;
using UnityEngine;

enum SheepState
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

    // Wander 
    private Vector3 _currentWanderTarget;
    [SerializeField] private float _wanderRadius = 2;
    private bool _waitingForNewTarget = true;

    public void Init(FlockManager flockManager)
    {
        _flockManager = flockManager;
    }

    void Start()
    {
        _state = SheepState.Wander;
        GetWanderTarget();
    }

    void Update()
    {
        if (_state == SheepState.Wander)
        {
            if (_waitingForNewTarget)
            {
                return;
            }
            Vector3 distanceFromTarget = _currentWanderTarget - transform.position;
            if (distanceFromTarget.magnitude <= 0.1f)
            {
                // Reached wander target, pick another
                _waitingForNewTarget = true;
                Invoke(nameof(GetWanderTarget), Random.Range(1,5));
                return;
            }

            Vector3 direction = distanceFromTarget.normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * _turnSpeed);
        }
    }

    private void GetWanderTarget()
    {
        _waitingForNewTarget = false;
        Vector3 currentHomingTarget = _flockManager.GetTargetPoint();
        Vector2 randomOffset = Random.insideUnitCircle * _wanderRadius;
        _currentWanderTarget = currentHomingTarget + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }
}
