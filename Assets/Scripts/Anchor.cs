using System;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    [SerializeField] private GameObject _light;

    public event Action OnTimerComplete;

    private FlockManager _flockManager;

    private bool _countdownStarted = false;
    private float _timer;

    private Collider _collider;

    void OnEnable()
    {
        _collider = GetComponent<Collider>();
        _flockManager = FindAnyObjectByType<FlockManager>();
        SetActiveState(false);
    }

    public void SetActiveState(bool active)
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
        if (_collider)
        {
            _collider.enabled = active;
        }

        _light.SetActive(active);
    }

    void Update()
    {
        if (_countdownStarted)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                SetActiveState(false);
                OnTimerComplete?.Invoke();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Wrymm>())
        {
            Debug.Log("Wyrmm entered");
        }
        if (other.GetComponent<Player>())
        {
            _countdownStarted = true;
            _timer = _lifeTime;
            _flockManager.SetHerdHomePoint(transform.position);
        }
    }
}
