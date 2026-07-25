using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyToSpawnPrefab;
    [SerializeField] private float _spawnInterval = 15f; // seconds

    // Instead of spawning on time interval, this one will check for total number of type in scene and spawn to reach that cap
    [SerializeField] private bool _spawnTotalCount = false;
    [SerializeField] private int _spawnCount = 1;

    private bool _active = false;
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetSpawningState;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= SetSpawningState;
    }

    private void Update()
    {
        if (!_active || !_spawnTotalCount)
        {
            return;
        }

        SpawnEnemyCount();
    }

    private void SpawnEnemyCount()
    {
        int currentCount = FindObjectsByType<Angler>().Length;
        while (currentCount < _spawnCount)
        {
            Instantiate(_enemyToSpawnPrefab, transform.position, transform.rotation);
            currentCount++;
        }
    }

    private void SetSpawningState(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.LevelStart:
                _active = true;
                if (!_spawnTotalCount)
                {
                    StartCoroutine(SpawnEnemy());
                }
                break;
            case GameManager.GameState.LevelComplete:
                _active = false;
                if (!_spawnTotalCount)
                {
                    StopCoroutine(SpawnEnemy());
                }
                break;
        }
    }

    private IEnumerator SpawnEnemy() 
    {
        while (true)
        {
            Instantiate(_enemyToSpawnPrefab, transform.position, transform.rotation);
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

}
