using System;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    [SerializeField] private float _lifeTime;

    public event Action OnTimerComplete;

    private bool _countdownStarted = false;
    private float _timer;

    private Collider _collider;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = false;
    }

    public void SetActiveState(bool active)
    {
        _collider.enabled = active;
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
        if (other.GetComponent<Player>())
        {
            _countdownStarted = true;
            _timer = _lifeTime;
        }
    }
}
