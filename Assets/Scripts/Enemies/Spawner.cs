using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyToSpawnPrefab;
    [SerializeField] private float _spawnInterval = 15f; // seconds

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += SetSpawningState;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= SetSpawningState;
    }

    private void SetSpawningState(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.LevelStart:
                StartCoroutine(SpawnEnemy());
                break;
            case GameManager.GameState.LevelComplete:
                StopCoroutine(SpawnEnemy());
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
