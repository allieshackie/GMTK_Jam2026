using System;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Tooltip("Sheep Spawn")]
    [SerializeField] private Sheep _sheepPrefab;

    [SerializeField] private int _spawnCount = 10;

    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private float _spawnRadius = 5;

    [SerializeField] private LayerMask _obstacleLayer;

    public event Action OnSheepKilled;

    public event Action LastSheepKilled;

    private List<Sheep> _flock = new List<Sheep>();

    private List<Lure> _lures = new List<Lure>();

    private Vector3 _herdHomePoint;

    private bool _homePointSet = false;

    void Start()
    {
        _herdHomePoint = _spawnPoint.position;
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 position = FindValidSpawnPosition();
            Sheep newSheep = Instantiate(_sheepPrefab, position, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));

            newSheep.Init(this);

            _flock.Add(newSheep);
        }
    }

    private Vector3 FindValidSpawnPosition()
    {
        const int maxAttempts = 50;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * _spawnRadius;
            Vector3 position = _spawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Check if this position is inside an obstacle
            if (!Physics.CheckSphere(position, 0.5f, _obstacleLayer))
            {
                return position;
            }
        }

        return _spawnPoint.position;
    }

    public Vector3 GetHerdHomePoint()
    {
        return _herdHomePoint;
    }

    public bool IsHomePointSet()
    {
        return _homePointSet;
    }

    public void SetHerdHomePoint(Vector3 newHomePoint)
    {
        _homePointSet = true;
        _herdHomePoint = newHomePoint;
    }

    public void UnsetHerdHomePoint()
    {
        _homePointSet = false;
    }

    public void AddLure(Lure lure)
    {
        _lures.Add(lure);
    }

    public void RemoveLure(Lure lure)
    {
        _lures.Remove(lure);
    }

    public List<Lure> GetCurrentLures()
    {
        return _lures;
    }

    public List<Sheep> GetCurrentFlock()
    {
        return _flock;
    }

    public int GetSheepCount()
    {
        return _flock.Count;
    }

    public int GetSpawnCount()
    {
        return _spawnCount;
    }

    public void RemoveSheep(Sheep sheep)
    {
        _flock.Remove(sheep);
        OnSheepKilled?.Invoke();
        Debug.Log($"flock count: ${_flock.Count}");
        if(_flock.Count == 0)
        {
            Debug.Log($"Last sheep killed");
            LastSheepKilled?.Invoke();
        }
    }

    public Sheep GetClosestSheep(Vector3 position, out float distance)
    {
        Sheep closest = null;
        distance = float.MaxValue;

        foreach (Sheep sheep in _flock)
        {
            float currentDistance = Vector3.Distance(position, sheep.transform.position);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = sheep;
            }
        }

        return closest;
    }

}
