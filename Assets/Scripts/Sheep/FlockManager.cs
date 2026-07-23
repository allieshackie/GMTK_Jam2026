using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Tooltip("Sheep Spawn")]
    [SerializeField] private Sheep _sheepPrefab;

    [SerializeField] private float _spawnCount = 10;

    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private float _spawnRadius = 5;

    private List<Sheep> _flock = new List<Sheep>();

    void Start()
    {
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * _spawnRadius;

            Vector3 position = _spawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
            Sheep newSheep = Instantiate(_sheepPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));

            newSheep.Init(this);

            _flock.Add(newSheep);
        }
    }

    void Update()
    {
        
    }

    public Vector3 GetTargetPoint()
    {
        return _spawnPoint.position;
    }

}
